<?php
namespace Events\Controller;

use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\AdminBaseController;

class AdminController extends AdminBaseController 
{
    use \Events\Logic\EventService;

    public function external_isAdmin() {
        if (!$this->isLogin()) {
            return false;
        }
        return $this->isSupervisor();
    }

    private function checkAdmin() {
        if (!$this->isSupervisor()) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
    }

    public function login() 
    {
        $account = $this->requestParameter('account');
        $password = $this->requestParameter('password');

        //TODO password need hash
        $cond = ['account' => $account, 'password' => $password];
        $adminsModel = $this->getModel("admins");
        $ret = $adminsModel->where($cond)->find();
        $this->checkDb($ret);
        if (empty($ret)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        if ($ret['level'] > 9) {
            $this->result(Constant::ERROR_PERM_DENY);
        }

        $data = ['last_login_time' => Utils::now(), 'last_login_ip' => Utils::getIpAddress()];
        $adminsModel->where(['id' => $ret['id']])->setField($data);

        $this->userLogin(['id' => $ret['id'], 'level' => $ret['level'], 'account' => $ret['account']]);

        $this->addlog("admin_login", $account);
        $this->OK();
    }

    public function logout() 
    {
        $this->userSessionClear();
        $this->OK();
    }

    public function event_list()
    {
        $this->checkLogin();
        $this->checkAdmin();

        $eventsModel = $this->getModel("events");
        $ret = $eventsModel->select();
        $this->checkDb($ret);
        
        $this->OK($ret);
    }

    public function event_add()
    {
        $this->checkLogin();
        $this->checkAdmin();
        
        $data = [
            'create_ts' => Utils::now(),
            'name' => $this->requestParameter('name'),
            'start_time' => $this->requestParameter('start_time'),
            'end_time' => $this->requestParameter('end_time'),
            'item_count' => $this->requestParameter('item_count'),
            'class_name' => $this->requestParameter('class_name'),
            'ticket_url' => $this->requestParameter('ticket_url'),
            'settle_url' => $this->requestParameter('settle_url'),
            'service_class' => $this->requestParameter('service_class'),
            'api_key' => Utils::genKey(),
            'sms_token' => $this->requestParameter('sms_token'),
        ];

        $eventsModel = $this->getModel("events");
        $ret = $eventsModel->add($data);
        $this->checkDb($ret);

        $this->addlog("admin_event_add", $ret);
        $this->OK();
    }

    public function event_update() 
    {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter('id');
        $data = [
            'name' => $this->requestParameter('name'),
            'start_time' => $this->requestParameter('start_time'),
            'end_time' => $this->requestParameter('end_time'),
            'item_count' => $this->requestParameter('item_count'),
            'class_name' => $this->requestParameter('class_name'),
            'ticket_url' => $this->requestParameter('ticket_url'),
            'settle_url' => $this->requestParameter('settle_url'),
            'service_class' => $this->requestParameter('service_class'),
            'sms_token' => $this->requestParameter('sms_token'),
        ];

        $eventsModel = $this->getModel("events");
        $ret = $eventsModel->where(['id' => $id])->setField($data);
        $this->checkDb($ret);
        
        $this->addlog("admin_event_update", $id);

        $this->OK();
    }

    public function event_onoff()
    {
        $this->checkLogin();
        $this->checkAdmin();
    
        $id = $this->requestParameter('id');
        $status = $this->requestParameter('status');

        $eventsModel = $this->getModel("events");
        $ret = $eventsModel->where(['id' => $id])->setField(['status' => $status]);
        $this->checkDb($ret);

        $this->addlog("admin_event_onoff", $id);

        $this->OK();
    }

    public function event_item_list() 
    {
        $this->checkLogin();

        $eid = $this->requestParameter("id");
        $itemModel = $this->getModel("event_items");
        $items = $itemModel->where(['event_id' => $eid])->select();
        $this->checkDb($items);
        
        $this->OK($items);
    }

    public function event_item_add() 
    {
        $this->checkLogin();
        $this->checkAdmin();

        $eid = $this->requestParameter('id');
        $data = [
            'create_ts' => Utils::now(),
            'event_id' => $eid,
            'type' => $this->requestParameter('type'),
            'name' => $this->requestParameter('name'),
            'code' => $this->requestParameter('code'),
            'secure' => $this->requestParameter('secure'),
            'allow_change' => $this->requestParameter('allow_change'),
            'allow_push' => $this->requestParameter('allow_push'),
        ];

        $eventsModel = $this->getModel("events");
        $ret = $eventsModel->where(['id' => $eid])->find();
        $this->checkDb($ret);

        $itemModel = $this->getModel("event_items");
        $count = $itemModel->where(['event_id' => $eid])->count();
        $this->checkDb($count);

        if ($count >= $ret['item_count']) {
            $this->result(Constant::ERROR_FAILED);
        }

        $ret = $itemModel->add($data);
        $this->checkDb($ret);

        $this->addlog("admin_event_item_add", $eid);

        $this->OK();
    }

    public function event_item_update() 
    {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter('id');

        $data = [
            'type' => $this->requestParameter('type'),
            'name' => $this->requestParameter('name'),
            'code' => $this->requestParameter('code'),
            'secure' => $this->requestParameter('secure'),
            'allow_change' => $this->requestParameter('allow_change'),
            'allow_push' => $this->requestParameter('allow_push'),
        ];

        $itemModel = $this->getModel("event_items");
        $ret = $itemModel->where(['id' => $id ])->setField($data);
        $this->checkDb($ret);

        $this->addlog("admin_event_item_update", $id);

        $this->OK();
    }

    public function event_item_remove() 
    {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter('id');

        $itemModel = $this->getModel("event_items");
        $ret = $itemModel->where(['id' => $id ])->delete();
        $this->checkDb($ret);

        $this->addlog("admin_event_item_remove", $id);

        $this->OK();
    }


    /*
    wechat admin
    */
    public function public_info() {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter("id");

        $wechatsModel = $this->getModel("event_wechats");
        $item = $wechatsModel->where(['id' => $id])->find();
        $this->checkDb($item);

        $this->OK($item);
    }

    public function public_set() {
        $this->checkLogin();
        $this->checkAdmin();

        $cond = [ 
            'id' => $this->requestParameter('id'),
            'create_ts' => Utils::now(),
        ];
        $data = [
            'appid' => $this->requestParameter('appid'),
            'appsecret' => $this->requestParameter('appsecret'),
            'aeskey' => $this->requestParameter('aeskey'),
            'token' => $this->requestParameter('token'),
            'class_name' => $this->requestParameter('class_name'),
            'status' => $this->requestParameter('status'),
            'boot_url' => $this->requestParameter('boot_url'),
            'scan_url' => $this->requestParameter('scan_url'),
        ];
        $wechatsModel = $this->getModel("event_wechats");
        $ret = $wechatsModel->add($cond + $data, '', $data);
        $this->checkDb($ret);

        $this->OK();
    }

    public function miniprog_list() {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter("id");
        if (empty($id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $miniModel = $this->getModel("event_miniprogs");
        $ret = $miniModel->where(['event_id' => $id])->select();
        $this->checkDb($ret);

        $this->OK($ret);
    }

    public function miniprog_add() {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter("id");
        if (empty($id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }
        
        $data = [
            'create_ts' => Utils::now(),
            'event_id' => $id,
            'app_id' => $this->requestParameter('app_id'),
            'appsecret' => $this->requestParameter('appsecret'),
        ];
        $miniModel = $this->getModel("event_miniprogs");
        $ret = $miniModel->add($data);
        $this->checkDb($ret);

        $this->OK();
    }

    public function miniprog_update() {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter("id");
        if (empty($id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $data = [
            'app_id' => $this->requestParameter('app_id'),
            'appsecret' => $this->requestParameter('appsecret'),
        ];

        $miniModel = $this->getModel("event_miniprogs");
        $ret = $miniModel->where(['id' => $id])->setField($data);
        $this->checkDb($ret);

        $this->OK($ret);
    }

    public function miniprog_remove() {
        $this->checkLogin();
        $this->checkAdmin();

        $id = $this->requestParameter("id");
        if (empty($id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }
        
        $miniModel = $this->getModel("event_miniprogs");
        $ret = $miniModel->where(['id' => $id])->delete();
        $this->checkDb($ret);

        $this->OK($ret);
    }

    public function item_auth_list()
    {
        $this->checkLogin();

        $id = $this->requestParameter('id');

        $itemsModel = $this->getModel('event_items');
        $item = $itemsModel->where(['id' => $id])->find();
        $this->checkDb($item);

        if (empty($item)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        if ($item['type'] == Constant::FLAG_NO) {
            $this->result(Constant::ERROR_FAILED);
        }

        $useritemModel = $this->getModel("user_event_items");
        $list = $useritemModel->where([
            'event_id' => $item['event_id'], 
            'item_id' => $id,
            'status' => Constant::STATUS_INIT,            
            ])->order(['update_ts' => 'asc'])->select();
        $this->checkDb($list);

        $eid = $item['event_id'];
        $service = $this->_getService($eid);
        foreach ($list as &$useritem) {
            $useritem['content'] = $service->filterItemContent($useritem['user_id'], $eid, $id, $item['code'], $useritem['content']);
        }

        $this->OK($list);
    }

    public function item_auth()
    {
        $this->checkLogin();
        $id = $this->requestParameter('id');

        if (empty($id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $status = $this->requestParameter('status');

        $useritemModel = $this->getModel("user_event_items");
        if (empty($status)) {
            $ret = $useritemModel->where(['id' => $id])->delete();
            $this->checkDb($ret);
        } else {
            $ret = $useritemModel->where(['id' => $id, 'status' => Constant::STATUS_INIT])->setField(['status' => Constant::STATUS_DONE, 'pstatus' => '0']);
            $this->checkDb($ret);

            $item = $useritemModel->where(['id'=>$id])->find();
            $this->checkDb($item);
            if (!empty($item)) {
                $eid = $item['event_id'];
                $item_id = $item['item_id'];
                $service = $this->_getService($eid);
                $itemcfg = $this->getItemById($item_id);
                $service->onAuth($item['user_id'], $eid, $item_id, $itemcfg['code'], $item['content']);
            }
        }

        $this->addlog("admin_item_auth", $id, $status);
        $this->OK();
    }

    public function stat()
    {
        $this->checkLogin();
        $eid = $this->requestParameter('id');

        $sql = $this->getEmptyModel();
        $result = $sql->query("SELECT item_id, code, name, count(*) as amount from user_items where event_id = $eid group by item_id;");
        $data['info'] = $result;

        $result = $sql->query("SELECT date(create_ts), item_id, code, name, count(*) as amount from user_items where event_id = $eid group by date(create_ts), item_id;");
        $data['details'] = $result;

        $event = $this->getEvent($eid);
        $service = $this->getService($event['service_class']);

        $custom = $service->getStatData($eid);
        $data['custom'] = $custom;

        $this->OK($data);
    }

    public function test() 
    {
        // var_dump($this->getItemById(9));
    }
}
