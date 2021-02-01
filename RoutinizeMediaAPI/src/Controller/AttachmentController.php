<?php
declare(strict_types=1);

namespace App\Controller;

use Cake\Datasource\ConnectionManager;

class AttachmentController extends AppController {

    private const ATTACHMENT_SIZE = 5000000; //5MB

    //Use Thread's ID for Container here
    public function saveAttachments() {
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
        $container = array_key_exists('container', $_REQUEST) ? $_REQUEST['container'] : null;
        $attachments = array_key_exists('attachments', $_FILES) ? $_FILES['attachments'] : null;

        if ($attachments != null && $hidrogenianId != null && $container != null) {
            $attachments = $this->reprocessMultipleImagesTempData($attachments);
            $atmPath = $this->createContainerForAtms($hidrogenianId, $container);

            $dbAtmNames = array();
            $oversizedAtms = array();
            $failedAtms = array();

            foreach ($attachments as $atm) {
                $message = $this->checkImageExif($atm, self::ATTACHMENT_SIZE);
                if (!empty($message)) {
                    array_push($oversizedAtms, $atm['name']);
                    continue;
                }

                $message = $this->saveImageToDisk($atm, $atmPath);
                if (!empty($message) && array_key_exists('error', $message)) {
                    array_push($failedAtms, $atm['name']);
                    continue;
                }

                $photo_newName = $message['imageName'];
                if ($atm['type'] != 'image/gif')
                    $this->reduceImageSize($atmPath.$photo_newName);

                $this->persistImageData($photo_newName, $hidrogenianId, $atmPath);
                array_push($dbAtmNames, [
                    'name' => $photo_newName,
                    'location' => $atmPath    
                ]);
            }

            $message = [
                'error' => !(!empty($dbAtmNames) && empty($oversizedAtms) && empty($failedAtms)),
                'errorMessage' => !(!empty($dbAtmNames) && empty($oversizedAtms) && empty($failedAtms)) ? 'interrupted' : null,
                'result' => [
                    'images' => $dbAtmNames,
                    'fails' => $failedAtms,
                    'oversizes' => $oversizedAtms
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
        //$this->set(compact('attachments', 'message'));
    }


    public function removeAttachments() {
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
        $container = array_key_exists('container', $_REQUEST) ? $_REQUEST['container'] : null;
        $removals = array_key_exists('removals', $_REQUEST) ? $_REQUEST['removals'] : null;

        $message = array();
        $failedRemovals = array();
        $unknownRemovals = array();

        if ($hidrogenianId != null && $removals != null) {
            $dbConnection = ConnectionManager::get('default');
            $container = $this->resembleAlbumPath($hidrogenianId, $container);

            foreach ($removals as $removal) {
                $counter = $dbConnection->execute('
                SELECT COUNT(*) AS PCount
                FROM Photos AS p, Userphotos AS u
                WHERE p.Id == u.PhotoId
                    AND u.HidrogenianId == ?
                    AND p.PhotoName == ?
                ', [$hidrogenianId, $removal])->fetch('assoc');

                if ($counter['PCount'] == 1) {
                    $message = $this->removeImageData($removal, false, $container);
                    if (!empty($message)) array_push($failedRemovals, $removal);
                }
                else array_push($unknownRemovals, $removal);
            }
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

    private function createContainerForAtms($hidrogenianId, $container) {
        $userDir = md5($hidrogenianId).'_'.time();
        $atmDir = md5($container).'_'.time();

        $this->createFolderIfNeeded(DS.'attachments'.DS.$userDir);
        $this->createFolderIfNeeded(DS.'attachments'.DS.$userDir.DS.$atmDir);

        return WWW_ROOT.'files'.DS.'gallery'.DS.$userDir.DS.$atmDir.DS;
    }
}
