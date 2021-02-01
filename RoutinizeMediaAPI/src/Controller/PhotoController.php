<?php
declare(strict_types=1);

namespace App\Controller;

use Cake\Datasource\ConnectionManager;

class PhotoController extends AppController {

    private const PHOTO_SIZE = 10000000; //10MB

    //Use Article's ID for Album here
    public function savePhotos() {
        $this->autoRender = false;
        $this->request->allowMethod(['post']);
        
        $response = $this->response;
        $response = $response->withType('application/json');
        
        $response = $this->response;
        $message = array();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response->withStringBody(json_encode($message));
            return $response;
        }

        $hidrogenianId = array_key_exists('hidrogenianId', $_REQUEST) ? $_REQUEST['hidrogenianId'] : null;
        $coverImage = array_key_exists('coverImage', $_REQUEST) ? $_REQUEST['coverImage'] : null;
        $album = array_key_exists('album', $_REQUEST) ? $_REQUEST['album'] : null;
        $images = array_key_exists('images', $_FILES) ? $_FILES['images'] : null;

        if ($images != null && $hidrogenianId != null && $coverImage != null && $album != null) {
            $images = $this->reprocessMultipleImagesTempData($images);
            $directoryPath = $this->createArticleFolderForUser($hidrogenianId, $album);

            $dbImageNames = array();
            $oversizedImages = array();
            $failedImages = array();

            foreach ($images as $image) {
                $message = $this->checkImageExif($image, self::PHOTO_SIZE);
                if (!empty($message)) {
                    array_push($oversizedImages, $image['name']);
                    continue;
                }

                $message = $this->saveImageToDisk($image, $directoryPath);
                if (!empty($message) && array_key_exists('error', $message)) {
                    array_push($failedImages, $image['name']);
                    continue;
                }

                $photo_newName = $message['imageName'];
                if ($image['type'] != 'image/gif')
                    $this->reduceImageSize($directoryPath.$photo_newName);

                $this->persistImageData($photo_newName, $hidrogenianId, $directoryPath, false, $coverImage == $image['name']);
                array_push($dbImageNames, [
                    'name' => $photo_newName,
                    'location' => $directoryPath    
                ]);
            }

            $message = [
                'error' => !(!empty($dbImageNames) && empty($oversizedImages) && empty($failedImages)),
                'errorMessage' => !(!empty($dbImageNames) && empty($oversizedImages) && empty($failedImages)) ? null : 'interrupted',
                'result' => [
                    'images' => $dbImageNames,
                    'fails' => $failedImages,
                    'oversizes' => $oversizedImages
                ]
            ];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process request due to missing data.',
                'result' => null
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
        //$this->set(compact('images', 'message'));
    }

    public function removePhotos() {
        $this->autoRender = false;
        $this->request->allowMethod(['post']);
        
        $response = $this->response;
        $message = array();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response->withStringBody(json_encode($message));
            return $response;
        }

        $hidrogenianId = array_key_exists('hidrogenianId', $_REQUEST) ? $_REQUEST['hidrogenianId'] : null;
        $album = array_key_exists('album', $_REQUEST) ? $_REQUEST['album'] : null;
        $removals = array_key_exists('removals', $_REQUEST) ? $_REQUEST['removals'] : null;

        $message = array();
        $failedRemovals = array();
        $unknownRemovals = array();

        if ($hidrogenianId != null && $removals != null) {
            $dbConnection = ConnectionManager::get('default');
            $albumPath = $this->resembleAlbumPath($hidrogenianId, $album);

            foreach ($removals as $removal) {
                $counter = $dbConnection->execute('
                SELECT COUNT(p.Id) AS PCount
                FROM Photos AS p, Userphotos AS u
                WHERE p.Id == u.PhotoId
                    AND u.HidrogenianId == ?
                    AND p.PhotoName == ?
                ', [$hidrogenianId, $removal])->fetch('assoc');

                if ($counter['PCount'] == 1) {
                    $message = $this->removeImageData($removal, false, $albumPath);
                    if (!empty($message)) array_push($failedRemovals, $removal);
                }
                else array_push($unknownRemovals, $removal);
            }

            if ($this->isFolderEmpty($albumPath)) $this->deleteAlbum($albumPath);
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process request due to missing data.',
                'result' => null
            ];

        $message = (!empty($message)) ? $message : [
            'error' => false,
            'errorMessage' => null,
            'result' => [
                'fails' => $failedRemovals,
                'unknowns' => $unknownRemovals
            ]
        ];
        
        $response = $response->withStringBody(json_encode($message));
        return $response;
        //$this->set(compact('removals', 'message'));
    }

    private function createArticleFolderForUser($hidrogenianId, $album) {
        $userDir = md5($hidrogenianId).'_'.time();
        $albumDir = md5($album).'_'.time();

        $this->createFolderIfNeeded(DS.'gallery'.DS.$userDir);
        $this->createFolderIfNeeded(DS.'gallery'.DS.$userDir.DS.$albumDir);

        return WWW_ROOT.'files'.DS.'gallery'.DS.$userDir.DS.$albumDir.DS;
    }
}
