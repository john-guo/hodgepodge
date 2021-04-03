<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IBackdoor;

class DefaultBackdoorService extends \Events\Controller\BackdoorController implements IBackdoor
{
    use \Events\Logic\EventService;

    public function play($event_id, $item_id, $uid) 
    {
        $this->_play($uid, $event_id, $item_id);
        $this->addbackdoorlog("play", $uid, $event_id, $item_id);
        $this->OK();
    }
}