<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IService;

class DefaultService extends \Common\Controller\CommonController implements IService
{
    public function onUserCreate($uid, $eid) 
    {
    }

    public function onRegister($uid, $eid) 
    {
    }

    public function getEnjoyAmount($uid, $eid, $code, $content) 
    {
        return 0;
    }

    public function onEnjoy($uid, $eid, $item_id, $code, $content)
    {

    }

    public function onPlay($uid, $eid, $item_id, $code, $content) 
    {
        
    }

    public function getTicketUrl($ticket_url, $code) 
    {
        return $ticket_url . "?code=" . $code;
    }

    public function getStatData($eid) 
    {
        return null;
    }

    public function onRegisterDetail($uid, $eid) {

    }

    public function onAuth($uid, $eid, $item_id, $code, $content)
    {
        
    }

    public function filterItemContent($uid, $eid, $item_id, $code, $content)
    {
        return $content;
    }
}