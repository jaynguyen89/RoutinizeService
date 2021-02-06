<?php
declare(strict_types=1);

namespace App\Controller;

class PhotoController extends AppController {

    private const COVER_SIZE = 3000000; //3MB
    private const GALLERY_DIR = WWW_ROOT.'files'.DS.'gallery'.DS;

    public function saveCover() {
        $this->autoRender = false;
        $this->request->allowMethod(['post']);

        $response = $this->response;
        $response = $response->withType('application/json');

        $result = $this->verifyApiKey();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $image = array();
        if (array_key_exists('image', $_FILES))
            $image = $_FILES['image'];

        $message = array();
        if (!empty($image)) $message = $this->checkImageExif($image, self::COVER_SIZE);
        if (!empty($message)) {
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;

        if (!empty($image) && $accountId != null) {
            $message = $this->saveImageToDisk($image, self::GALLERY_DIR);
            if (!empty($message) && array_key_exists('error', $message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $cover_newName = $message['imageName'];
            if ($image['type'] != 'image/gif')
                $this->reduceImageSize(self::GALLERY_DIR.$cover_newName);

            $this->persistImageData($cover_newName, $accountId, self::GALLERY_DIR, true);
            $message = [
                'error' => false,
                'errorMessage' => null,
                'result' => [
                    'name' => $cover_newName,
                    'location' => self::GALLERY_DIR
                ]
            ];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process your request due to missing data.',
                'result' => null
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
        //$this->set(compact('image', 'message'));
    }


    public function replaceCover() {
        $this->autoRender = false;
        $this->request->allowMethod(['post']);

        $response = $this->response;
        $response = $response->withType('application/json');

        $result = $this->verifyApiKey();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $currentCover = array_key_exists('current', $_REQUEST) ? $_REQUEST['current'] : null;
        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;
        $newCover = array_key_exists('replaceBy', $_FILES) ? $_FILES['replaceBy'] : null;

        if ($currentCover != null && $newCover != null && $accountId != null) {
            $message = $this->removeImageData($currentCover, true);
            if (!empty($message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $message = $this->checkImageExif($newCover, self::COVER_SIZE);
            if (!empty($message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $message = $this->saveImageToDisk($newCover, self::GALLERY_DIR);
            if (!empty($message) && array_key_exists('error', $message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $cover_newName = $message['imageName'];
            if ($newCover['type'] != 'image/gif')
                $this->reduceImageSize(self::GALLERY_DIR.$cover_newName);

            $this->persistImageData($cover_newName, $accountId, self::GALLERY_DIR, true);
            $message = [
                'error' => false,
                'errorMessage' => null,
                'result' => [
                    'name' => $cover_newName,
                    'location' => self::GALLERY_DIR
                ]
            ];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process your request due to missing data.',
                'result' => null
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
        //$this->set(compact('currentCover', 'newCover', 'message'));
    }


    public function removeCover() {
        $this->autoRender = false;
        $this->request->allowMethod(['post']);

        $response = $this->response;
        $response = $response->withType('application/json');

        $result = $this->verifyApiKey();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $imageName = array_key_exists('image', $_REQUEST) ? $_REQUEST['image'] : null;

        if ($imageName != null) {
            $message = $this->removeImageData($imageName, false, self::GALLERY_DIR);
            if (!empty($message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $message = ['error' => false, 'errorMessage' => null, 'result' => null];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'No data to process your request.',
                'result' => null
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
        //$this->set(compact('message', 'imageName'));
    }
}
