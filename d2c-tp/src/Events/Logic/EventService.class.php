<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;

trait EventService 
{
    private function getUser($uid) 
    {
        $userModel = $this->getModel('users');
        $user = $userModel->where(['id' => $uid])->find();
        $this->checkDb($user);
        
        if (empty($user)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        return $user;
    }

    private function getEvent($eid) 
    {
        $eventModel = $this->getModel('events');
        $event = $eventModel->where(['id' => $eid])->find();
        $this->checkDb($event);
        
        if (empty($event)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        return $event;
    }

    private function getService($service_class) 
    {
        $obj = null;
        if (!empty($service_class)) {
            $obj = new $service_class;
        }
        if ($obj == null) {
            $obj = new \Events\Logic\DefaultService;
        }

        return $obj;
    }

    private function _getService($eid) {
        $event = $this->getEvent($eid);
        return $this->getService($event['service_class']);
    }

    private function getItem($eid, $code) 
    {
        $itemModel = $this->getModel("event_items");
        $item = $itemModel->where(['code' => $code, 'event_id' => $eid])->find();
        $this->checkDb($item);

        if (empty($item)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        return $item;
    }

    private function getItemById($item_id) {
        $itemModel = $this->getModel("event_items");
        $item = $itemModel->where(['id' => $item_id])->find();
        $this->checkDb($item);

        if (empty($item)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        return $item;
    }

    private function _enjoy($uid, $code, $content) 
    {
        $user = $this->getUser($uid);
        
        $eid = $user['event_id'];        
        $event = $this->getEvent($eid);
        $service = $this->getService($event['service_class']);        
        $itemcfg = $this->getItem($eid, $code);

        if ($user['status'] == Constant::STATUS_DONE) {
            $this->result(Constant::ERROR_USER_ALREADY_SETTLE);
        }

        $id = $itemcfg['id'];
        $useritemModel = $this->getModel('user_event_items');
        $cond = [
            'user_id' => $uid,
            'event_id' => $eid,
            'item_id' => $id
        ];
        $item = $useritemModel->where($cond)->find();
        $this->checkDb($item);

        $status = Constant::STATUS_INIT;

        if (empty($item)) 
        {
            $amount = $service->getEnjoyAmount($uid, $eid, $code, $content);

            $ret = $useritemModel->add([
                'create_ts' => Utils::now(),
                'user_id' => $uid,
                'event_id' => $eid,
                'item_id' => $id,
                'amount' => $amount,
                'content' => $content,
                'status' => $status,
            ]);
            $this->checkDb($ret);

            $service->onEnjoy($uid, $eid, $id, $code, $content);

            $this->addlog('enjoy', $eid, $id, $ret);
        } 
        else if ($itemcfg['allow_change'] == Constant::FLAG_YES) 
        {
            if ($item['status'] == Constant::STATUS_DONE) {
                $this->result(Constant::ERROR_USER_ITEM_DONE);
            }
            if ($itemcfg['type'] == Constant::TYPE_AUTH && $item['status'] == Constant::STATUS_INIT) {
                $this->result(Constant::ERROR_USER_AUTHORING);
            }

            $ret = $useritemModel->where($cond + ['status' => ['neq', Constant::STATUS_DONE]])->setField(['content' => $content, 'status' => $status, 'pstatus' => '0']);
            $this->checkDb($ret);

            $service->onEnjoy($uid, $eid, $id, $code, $content);

            $this->addlog('enjoy', $eid, $id, $item['id']);
        } else {
            $this->result(Constant::ERROR_USER_ALREADY_ENJOY);
        }

        $item = $useritemModel->where($cond)->find();
        $this->checkDb($item);
        return $item;
    }

    private function _play($uid, $event_id, $item_id) {
        $event = $this->getEvent($event_id);
        $service = $this->getService($event['service_class']);
        $itemsModel = $this->getModel("event_items");
        $itemcfg = $itemsModel->where(['id' => $item_id])->find();
        $this->checkDb($itemcfg);
        $code = $itemcfg['code'];

        if ($user['status'] == Constant::STATUS_DONE) {
            $this->result(Constant::ERROR_USER_ALREADY_SETTLE);
        }

        $cond = ['user_id' => $uid, 'event_id' => $event_id, 'item_id' => $item_id];
        $data = ['status' => Constant::STATUS_DONE];
        $itemsModel = $this->getModel("user_event_items");
        $item = $itemsModel->where($cond)->find();
        $this->checkDb($item);
        $uiid = $item['id'];
        $content = $item['content'];

        // if ($itemcfg['allow_push'] == Constant::FLAG_YES && $item['pstatus'] == Constant::FLAG_NO) {
        //     $this->result(Constant::ERROR_ITEM_NOT_COMPLETE);
        // }

        if (empty($item)) {
            $uiid = $itemsModel->add($cond + $data + [ 'create_ts' => Utils::now() ], [], $data);
            $this->checkDb($uiid);
        } else if ($item['status'] == Constant::STATUS_INIT) {
            $ret = $itemsModel->where($cond + ['status' => Constant::STATUS_INIT])->setField($data);
            $this->checkDb($ret);
        } else {
            $this->result(Constant::ERROR_USER_ITEM_STATUS_WRONG);
        }

        $service->onPlay($uid, $event_id, $item_id, $code, $content);
        $this->addlog('play', $event_id, $item_id, $uiid);
    }

    private function _settle($uid) 
    {
        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $event = $this->getEvent($eid);

        if ($user['status'] == Constant::STATUS_DONE) {
            $this->result(Constant::ERROR_USER_ALREADY_SETTLE);
        }

        $itemModel = $this->getModel('user_event_items');
        $count = $itemModel->where([
            'user_id' => $uid,
            'event_id' => $eid,
            'status' => Constant::STATUS_DONE,
        ])->count();
        $this->checkDb($count);
        
        if ($count != $event['item_count']) {
            $this->result(Constant::ERROR_USER_ITEMS_NOT_MATCH);
        }

        $userModel = $this->getModel('users');
        $ret = $userModel->where(['id' => $uid, 'status' => Constant::STATUS_INIT])->setField(['status' => Constant::STATUS_DONE]);
        $this->checkDb($ret);

        if (empty($ret)) {
            $this->result(Constant::ERROR_USER_ALREADY_SETTLE);
        }

        $this->addlog("settle", $eid, $uid);
    }
}
