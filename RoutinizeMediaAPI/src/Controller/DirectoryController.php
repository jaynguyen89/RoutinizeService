<?php
declare(strict_types=1);

namespace App\Controller;

use Cake\ORM\TableRegistry;

class DirectoryController extends AppController {

    //Param $dir is the path to folder
    public function cleanEmptyDirectories() {
        $this->autoRender = false;
        $this->request->allowMethod(['delete']);
        
        $response = $this->response;
        $response = $response->withType('application/json');
        
        $response = $this->response;
        $message = array();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response->withStringBody(json_encode($message));
            return $response;
        }

        $dir = array_key_exists('directory', $_REQUEST) ? $_REQUEST['directory'] : null;

        $filesFolder = WWW_ROOT.'files'.DS.$dir;
        $this->cleanDirInternal($filesFolder);

        $message = ['error' => false];
        $response = $response->withStringBody(json_encode($message));
        return $response;
    }

    public function deleteUserDirectoriesAndFilesUnsafe() {
        $this->autoRender = false;
        $this->request->allowMethod(['delete']);
        
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

        $this->deleteAvatarUnsafe($hidrogenianId);

        $userFolderPrefix = md5($hidrogenianId);
        $attachmentFolders = glob(WWW_ROOT.'files'.DS.'attachments'.DS.'*', GLOB_MARK);

        if ($hidrogenianId != null) {
            foreach ($attachmentFolders as $folder)
                if (strpos($folder, $userFolderPrefix) !== false) {
                    $this->deleteAlbum($folder);
                    break;
                }

            $galleryFolders = glob(WWW_ROOT . 'files' . DS . 'gallery' . DS . '*', GLOB_MARK);
            foreach ($galleryFolders as $folder)
                if (strpos($folder, $userFolderPrefix) !== false) {
                    $this->deleteAlbum($folder);
                    break;
                }

            $message = ['error' => false];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process your request due to missing data.'
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
    }

    //Need review
    public function safeDeleteAlbum() {
        $this->autoRender = false;
        $this->request->allowMethod(['delete']);
        
        $response = $this->response;
        $message = array();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response->withStringBody(json_encode($message));
            return $response;
        }

        $hidrogenianId = array_key_exists('hidrogenianId', $_REQUEST) ? $_REQUEST['hidrogenianId'] : null;
        $album = array_key_exists('album', $_REQUEST) ? $_REQUEST['album'] : null;
        $folder = array_key_exists('folder', $_REQUEST) ? $_REQUEST['folder'] : null; //gallery for attachments

        if ($hidrogenianId != null && $album != null) {
            $userFolderPrefix = md5($hidrogenianId);

            $galleryFolders = glob(WWW_ROOT.'files'.DS.$folder.DS.'*', GLOB_MARK);
            foreach ($galleryFolders as $folder)
                if (strpos($folder, $userFolderPrefix) !== false) {
                    $albumPrefix = md5($album);

                    $albumFolders = glob($folder.'*', GLOB_MARK);
                    foreach ($albumFolders as $al) {
                        if (strpos(al, $albumPrefix))
                            $this->deleteAlbum($al);

                        break;
                    }

                    break;
                }

            $message = ['error' => false];
        }
        else
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to process your request due to missing data.'
            ];

        $response = $response->withStringBody(json_encode($message));
        return $response;
    }

    private function deleteAvatarUnsafe($hidrogenianId) {
        $userPhoto = TableRegistry::getTableLocator()->get('Userphotos')
            ->where(['HidrogenianId' => $hidrogenianId, 'IsAvatar' => true])->first();

        $avatar = TableRegistry::getTableLocator()->get('Photos')
            ->get(['Id' => $userPhoto->PhotoId]);

        unlink($avatar->Location.$avatar->PhotoName);
        TableRegistry::getTableLocator()->get('Photos')->delete($avatar);
    }

    private function cleanDirInternal($folderPath) {
        if (substr($folderPath, strlen($folderPath) - 1, 1) != DS)
            $folderPath .= DS;

        $files = glob($folderPath.'*', GLOB_MARK);

        foreach ($files as $file)
            if (is_dir($file) && $this->isFolderEmpty($file))
                rmdir($file);
            elseif (is_dir($file) && !$this->isFolderEmpty($file))
                self::cleanDirInternal($file);
    }
}
