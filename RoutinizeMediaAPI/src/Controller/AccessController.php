<?php
declare(strict_types=1);

namespace App\Controller;

class AccessController extends AppController {

    public function filterResult() {
        $this->autoRender = false;
        
        $message = array();
        $result = $this->request->getQuery('result');
        switch ($result) {
            case self::RESULTS['NO_KEY']:
                $message = [
                    'error' => true,
                    'errorMessage' => 'Unable to read the API Key from request. Please reload page and try again.'
                ];
                break;
            case self::RESULTS['NOT_FOUND']:
                $message = [
                    'error' => true,
                    'errorMessage' => 'The API key from request matches nothing in our records. Please check again.'
                ];
                break;
            case self::RESULTS['EXPIRED']:
                $message = [
                    'error' => true,
                    'errorMessage' => 'The API Key from your request seems to be expired. Please reload page and try again.'
                ];
                break;
            case self::RESULTS['RETARGET']:
                $message = [
                    'error' => true,
                    'errorMessage' => 'Your request falls outside our scope. Please reload page and try again.'
                ];
                break;
            default:
                $message = [
                    'error' => true,
                    'errorMessage' => 'An error occurred while processing your request. Please reload page and try again.'
                ];
                break;
        }

        $response = $this->response;
        $response = $response->withType('application/json');
        $response = $response->withStringBody(json_encode($message));
        return $response;
    }
}
