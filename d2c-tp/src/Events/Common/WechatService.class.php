<?php
namespace Events\Common;
use \Common\Common\Utils;
use \Common\Controller\WechatController;
abstract class WechatService extends WechatController implements IWechat
{
    function __construct($event_id) {
        $this->currentEvent = $event_id;
    }

    protected function getTicketContent($id) {
        $ticketModel = $this->getModel("event_tickets");
        $ticket = $ticketModel->where(array('id' => $id))->find();
        if ($ticket === false) {
            $this->weixinlog("db failed", "getTicketContent", $this->currentEvent, '', '');
            return null;
        }
        if (empty($ticket)) {
            $this->weixinlog("ticket not found", "getTicketContent", $this->currentEvent, '', '');
            return null;
        }

        return $ticket['content'];
    }

    public function onUserScan($user, $code) {
        return null;
    }
      
    public function onMenuClick($key) {
        return null;
    }
 
    public function getAutoReplyConfig() {
        return null;
    }
  
    public function getGreetingMsg($user) {
        return null;
    }
  
    public function getMenuConfig() {
        return null;
    }
}