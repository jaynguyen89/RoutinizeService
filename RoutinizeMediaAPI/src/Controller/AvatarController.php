<?php
declare(strict_types=1);

namespace App\Controller;

class AvatarController extends AppController {

    private const IMAGE_SIZE = 3000000; //3MB
    private const AVATAR_DIR = WWW_ROOT.'files'.DS.'avatars'.DS;

    public function saveAvatar() {
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
        if (!empty($image)) $message = $this->checkImageExif($image, self::IMAGE_SIZE);
        if (!empty($message)) {
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;

        if (!empty($image) && $accountId != null) {
            $message = $this->saveImageToDisk($image);
            if (!empty($message) && array_key_exists('error', $message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $avatar_newName = $message['imageName'];
            if ($image['type'] != 'image/gif')
                $this->reduceImageSize(self::AVATAR_DIR.$avatar_newName);

            $this->persistImageData($avatar_newName, $accountId, self::AVATAR_DIR, true);
            $message = [
                'error' => false,
                'errorMessage' => null,
                'result' => [
                    'name' => $avatar_newName,
                    'location' => self::AVATAR_DIR
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


    public function replaceAvatar() {
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

        $currentAvatar = array_key_exists('current', $_REQUEST) ? $_REQUEST['current'] : null;
        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;
        $newAvatar = array_key_exists('replaceBy', $_FILES) ? $_FILES['replaceBy'] : null;

        if ($currentAvatar != null && $newAvatar != null && $accountId != null) {
            $message = $this->removeImageData($currentAvatar, true);
            if (!empty($message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $message = $this->checkImageExif($newAvatar, self::IMAGE_SIZE);
            if (!empty($message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $message = $this->saveImageToDisk($newAvatar);
            if (!empty($message) && array_key_exists('error', $message)) {
                $response = $response->withStringBody(json_encode($message));
                return $response;
            }

            $avatar_newName = $message['imageName'];
            if ($newAvatar['type'] != 'image/gif')
                $this->reduceImageSize(self::AVATAR_DIR.$avatar_newName);

            $this->persistImageData($avatar_newName, $accountId, self::AVATAR_DIR, true);
            $message = [
                'error' => false,
                'errorMessage' => null,
                'result' => [
                    'name' => $avatar_newName,
                    'location' => self::AVATAR_DIR
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
        //$this->set(compact('currentAvatar', 'newAvatar', 'message'));
    }


    public function removeAvatar() {
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
            $message = $this->removeImageData($imageName, true);
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
