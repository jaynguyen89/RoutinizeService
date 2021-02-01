<?php
declare(strict_types=1);
namespace App\Controller;

use Cake\Core\Configure;
use Cake\Controller\Controller;
use Cake\Event\EventInterface;
use Cake\ORM\TableRegistry;
use DateTime;
use Exception;

class AppController extends Controller {

    public const RESULTS = [
        'NO_KEY' => 'ApiKeyMissing',
        'NOT_FOUND' => 'DbKeyNotFound',
        'EXPIRED' => 'ApiKeyExpired',
        'RETARGET' => 'WrongApiTarget'
    ];

    public function initialize(): void {
        parent::initialize();

        $this->loadComponent('RequestHandler');
        $this->loadComponent('Flash');
    }

    public function beforeFilter(EventInterface $event) {
        parent::beforeFilter($event);
        Configure::write('debug', false);
    }
    
    public function verifyApiKey() {
        $result = '';

        $apiKey = array_key_exists('apikey', $_REQUEST) ? $_REQUEST['apikey'] : null;
        if ($apiKey == null) $result = 'NO_KEY';
        
        $dbKey = TableRegistry::getTableLocator()->get('Tokens')
            ->find()->where(['TokenString' => $apiKey == null ? '' : $apiKey])->first();

        if ($dbKey == null) $result = 'NOT_FOUND';
        elseif ($dbKey->TimeStamp->modify('+' . $dbKey->Life . ' minutes') < new DateTime())
            $result = 'EXPIRED';
        else {
            $queryCtrlr = $this->request->getParam('controller');
            $queryAction = $this->request->getParam('action');
    
            $tokens = explode('/', $dbKey->Target);
            if (strtolower($queryCtrlr) != strtolower($tokens[0]) || strtolower($queryAction) != strtolower($tokens[1]))
                $result = 'RETARGET';
        }
        
        return $result;
    }
    
    public function filterResult($result) {
        $message = array();
        
        if ($result == 'NO_KEY')
            $message = [
                'error' => true,
                'errorMessage' => 'Unable to read the API Key from request. Please reload page and try again.'
            ];
                
        if ($result == 'NOT_FOUND')
            $message = [
                'error' => true,
                'errorMessage' => 'The API key from request matches nothing in our records. Please check again.'
            ];
                
        if ($result == 'EXPIRED')
            $message = [
                'error' => true,
                'errorMessage' => 'The API Key from your request seems to be expired. Please reload page and try again.'
            ];
                
        if ($result == 'RETARGET')
            $message = [
                'error' => true,
                'errorMessage' => 'Your request falls outside our scope. Please reload page and try again.'
            ];
                
        return $message;
    }

    /**
     * Turns the array of uploaded images'info into an array of images.
     * @param $tempImages
     * @return array
     */
    public function reprocessMultipleImagesTempData($tempImages) {
        $images = array();
        $imageCount = count($tempImages['name']);

        for ($i = 0; $i < $imageCount; $i++) {
            $anImage = array();
            $anImage['name'] = $tempImages['name'][$i];
            $anImage['type'] = $tempImages['type'][$i];
            $anImage['tmp_name'] = $tempImages['tmp_name'][$i];
            $anImage['error'] = $tempImages['error'][$i];
            $anImage['size'] = $tempImages['size'][$i];

            array_push($images, $anImage);
        }

        return $images;
    }

    /**
     * Check image type and size. For gif, limit size to a fixed 3MB.
     * @param $image
     * @param $size
     * @return array
     */
    public function checkImageExif($image, $size) {
        $allowTypes = ['image/jpg', 'image/jpeg', 'image/png', 'image/gif'];
        $message = array();

        if (!in_array($image['type'], $allowTypes))
            $message = [
                'error' => true,
                'errorMessage' => 'The photo is not of expected type. Expected: JPG, JPEG, PNG, GIF.'
            ];

        if ($image['type'] == 'image/gif') {
            if ($image['size'] > 3000000)
                $message = [
                    'error' => true,
                    'errorMessage' => 'The GIF photo is too big. Max size allowed: 3MB.'
                ];
        }
        else {
            if ($image['size'] > $size)
                $message = [
                    'error' => true,
                    'errorMessage' => 'The photo is too big. Max size allowed: ' . $size . 'MB.'
                ];
        }

        return $message;
    }

    /**
     * Compress images (except GIF) by 50% size
     * @param $imagePath
     * @return bool
     */
    public function reduceImageSize($imagePath) {
        $type = getimagesize($imagePath);
        $image = null;

        if ($type['mime'] == 'image/jpg' || $type['mime'] == 'image/jpeg')
            $image = imagecreatefromjpeg($imagePath);

        if ($type['mime'] == 'image/png')
            $image = imagecreatefrompng($imagePath);

        return imagejpeg($image, $imagePath, 50);
    }

    /**
     * Save the uploaded image from temp folder to disk
     * @param $image
     * @param null $album
     * @return array
     */
    public function saveImageToDisk($image, $album = null) {
        $image_newName = '';
        try {
            $imageName = strtolower($image['name']);
            $tokens = explode('.', $imageName);
            $imageExtension = end($tokens);

            $image_newName = md5($imageName) . '_' . time() . '.' . $imageExtension;
            move_uploaded_file(
                $image['tmp_name'],
                $album != null ? $album.$image_newName
                    : WWW_ROOT.'files'.DS.'avatars'.DS.$image_newName
            );
            chmod(WWW_ROOT.'files'.DS.($album == null ? 'avatars' : 'gallery'.DS.$album).DS.$image_newName, 0755);
        } catch (Exception $e) {
            $message = [
                'error' => true,
                'errorMessage' => 'An error occurred while attempting to save new image. Please try again.'
            ];
        }

        $message = !empty($message) ? $message : ['imageName' => $image_newName];
        return $message;
    }

    /**
     * Insert database entries for Photos and Userphotos tables
     * @param $imageName
     * @param $userId
     * @param $location
     * @param bool $isAvatar
     * @param bool $isCover
     */
    public function persistImageData($imageName, $userId, $location, $isAvatar = false, $isCover = false) {
        $dbPhoto = TableRegistry::getTableLocator()->get('Photos')->newEmptyEntity();
        $dbPhoto->PhotoName = $imageName;
        $dbPhoto->Location = $location;
        TableRegistry::get('photos')->save($dbPhoto);

        $dbUserPhoto = TableRegistry::getTableLocator()->get('Userphotos')->newEmptyEntity();
        $dbUserPhoto->PhotoId = $dbPhoto->Id;
        $dbUserPhoto->HidrogenianId = intval($userId);
        $dbUserPhoto->IsAvatar = $isAvatar;
        $dbUserPhoto->IsCover = $isCover;
        TableRegistry::getTableLocator()->get('Userphotos')->save($dbUserPhoto);
    }

    /**
     * Remove database entry associated with the image, then delete image on disk
     * @param $imageName
     * @param bool $isAvatar
     * @param null $album
     * @return array
     */
    public function removeImageData($imageName, $isAvatar = false, $album = null) {
        $message = array();
        try {
            $currentDbImage = TableRegistry::getTableLocator()->get('Photos')->find()->where(['PhotoName' => $imageName])->first();
            TableRegistry::getTableLocator()->get('Photos')->delete($currentDbImage);

            unlink(
                $album != null ? $album.$imageName
                : WWW_ROOT.'files'.DS.'avatars'.DS.$imageName
            );
        } catch (Exception $e) {
            $message = [
                'error' => true,
                'errorMessage' => 'An error occurred while attempting to replace your image. Please try again.'
            ];
        }

        return $message;
    }

    /**
     * Only create the folder if it is not existed.
     * @param $folderPath
     * @param int $permission
     */
    public function createFolderIfNeeded($folderPath, $permission = 0755) {
        if (!is_dir(WWW_ROOT.'files'.$folderPath))
            mkdir(WWW_ROOT.'files'.$folderPath, $permission, true);
    }

    public function isFolderEmpty($folderPath) {
        $handle = opendir($folderPath);

        while (false !== ($entry = readdir($handle)))
            if ($entry != "." && $entry != "..") {
                closedir($handle);
                return false;
            }

        closedir($handle);
        return true;
    }

    public function deleteAlbum($albumPath) {
        if (substr($albumPath, strlen($albumPath) - 1, 1) != DS)
            $albumPath .= DS;

        $files = glob($albumPath.'*', GLOB_MARK);
        foreach ($files as $file)
            if (is_dir($file)) self::deleteAlbum($file);
            else unlink($file);

        rmdir($albumPath);
    }

    public function resembleAlbumPath($hidrogenianId, $album) {
        $userDir = md5($hidrogenianId).'_'.time();
        $albumDir = md5($album).'_'.time();

        return WWW_ROOT.'files'.DS.'gallery'.DS.$userDir.DS.$albumDir.DS;
    }
}
