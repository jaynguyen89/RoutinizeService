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

        $result = $this->verifyApiKey();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;
        $container = array_key_exists('container', $_REQUEST) ? $_REQUEST['container'] : null;
        $attachments = array_key_exists('attachments', $_FILES) ? $_FILES['attachments'] : null;

        if ($attachments != null && $accountId != null && $container != null) {
            $attachments = $this->reprocessMultipleImagesTempData($attachments);
            $atmPath = $this->createContainerForAtms($accountId, $container);

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

                $atm_newName = $message['imageName'];
                if (strpos($atm['type'], 'image') && $atm['type'] != 'image/gif')
                    $this->reduceImageSize($atmPath.$atm_newName);

                $this->persistImageData($atm_newName, $accountId, $atmPath);
                array_push($dbAtmNames, [
                    'name' => $atm_newName,
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

        $result = $this->verifyApiKey();
        if (strlen($result) != 0) {
            $message = $this->filterResult($result);
            $response = $response->withStringBody(json_encode($message));
            return $response;
        }

        $accountId = array_key_exists('accountId', $_REQUEST) ? $_REQUEST['accountId'] : null;
        $container = array_key_exists('container', $_REQUEST) ? $_REQUEST['container'] : null;
        $removals = array_key_exists('removals', $_REQUEST) ? $_REQUEST['removals'] : null;

        $message = array();
        $failedRemovals = array();
        $unknownRemovals = array();

        if ($accountId != null && $removals != null) {
            $dbConnection = ConnectionManager::get('default');
            $container = $this->resembleAlbumPath($accountId, $container);

            foreach ($removals as $removal) {
                $counter = $dbConnection->execute('
                SELECT COUNT(*) AS PCount
                FROM Photos AS p, Userphotos AS u
                WHERE p.Id == u.PhotoId
                    AND u.accountId == ?
                    AND p.PhotoName == ?
                ', [$accountId, $removal])->fetch('assoc');

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

    private function createContainerForAtms($accountId, $container) {
        $userDir = md5($accountId).'_'.time();
        $atmDir = md5($container).'_'.time();

        $this->createFolderIfNeeded(DS.'attachments'.DS.$userDir);
        $this->createFolderIfNeeded(DS.'attachments'.DS.$userDir.DS.$atmDir);

        return WWW_ROOT.'files'.DS.'gallery'.DS.$userDir.DS.$atmDir.DS;
    }
}
