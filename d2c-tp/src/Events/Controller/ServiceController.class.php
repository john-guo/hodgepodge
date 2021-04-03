<?php
namespace Events\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;
class ServiceController extends \Common\Controller\CommonController 
{
    use \Events\Logic\EventService;

    //const SMS_TOKEN = "7100239230898406";

    public function get_captcha()  
    {
        $this->captcha();
        exit;
    }

    public function sms() 
    {
        $eid = $this->requestParameter('event_id');
        $mobile = $this->requestParameter('mobile');
        $event = $this->getEvent($eid);
        if (empty($mobile)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $prevcode = $this->map_retrieve($mobile);
        if (!empty($prevcode)) {
            $this->result(Constant::ERROR_PERM_DENY);
        }

        $token = $event['sms_token'];
        $code = $this->retrieveCode($mobile);
        $result = Utils::sendSms($mobile, $code, [Constant::SMS_OPTIONS_TOKEN => $token]);
        if ($result !== true) {
            $this->result(Constant::ERROR_FAILED, $result);
        }

        $this->OK();
    }

    private function _wechatlogin($open_id, $event_id) {
        if (empty($open_id)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }
        $event = $this->getEvent($event_id);
        $userModel = $this->getModel("users");
        $user = $userModel->where(['open_id' => $open_id, 'event_id' => $event_id])->find();
        $this->checkDb($user);

        $account = '';
        $status = 1;
        if (empty($user)) {
            $ret = $userModel->add([
                'create_ts' => Utils::now(), 
                'open_id' => $open_id,
                'event_id' => $event_id
            ]);
            $this->checkDb($ret);
            $uid = $ret;

            $service = $this->_getService($event_id);
            $service->onUserCreate($uid, $event_id);

            $this->addlog("usercreate", $event_id, $open_id);
        } else {
            $uid = $user['id'];
            $account = $user['mobile'];
            $status = empty($user['mobile']) ? 1 : 0;
        }
        
        $this->userLogin([
            'id' => $uid,
            'account' => $account,
            'nickname' => $account,
        ]);

        $this->addlog("wechatlogin", $event_id, $open_id);
        $this->OK(['status' => $status]);
    }

    private function checkAccount() {
        $uid = $this->currentUid();
        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $event = $this->getEvent($eid);

        $neednotcheck = empty($event['need_userinfo']);
        if ($neednotcheck) {
            return;
        }

        $account = $this->currentAccount();
        $this->addlog("checkAccount", $eid, $account);
        if (empty($account)) {
            $this->result(Constant::ERROR_USER_NOT_REGISTER);
        }
    }

    public function wechatlogin()
    {
        $open_id = $this->requestParameter("open_id");
        $event_id = $this->requestParameter("event_id");

        $data = Utils::get_parameter($open_id);
        if (empty($data)) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
        $open_id = $data['uid'];

        $this->_wechatlogin($open_id, $event_id);
    }

    public function custom_wechatlogin() 
    {
        $key = $this->requestParameter('key');
        $open_id = $this->requestParameter("open_id");
        $event_id = $this->requestParameter("event_id");
        $this->checkApi($event_id, $key);
        $this->_wechatlogin($open_id, $event_id);
    }

    public function wechatregister() 
    {
        $this->checkLogin();
        $uid = $this->currentUid();
        $mobile = $this->requestParameter("mobile");
        $code = $this->requestParameter("code");
        $name = $this->requestParameter('name');
        $birthday = $this->requestParameter('birthday');

        if (!$this->checkCode($mobile, $code)) {
            $this->result(Constant::ERROR_USER_SMS);
        }

        $userModel = $this->getModel("users");
        $user = $userModel->where(['id' => $uid])->find();
        $this->checkDb($user);

        if (empty($user)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        if (!empty($user['mobile'])) {
            $this->result(Constant::ERROR_USER_ALREADY_REGISTER);
        }

        $eid = $user['event_id'];
        $userModel = $this->getModel("users");
        $ret = $userModel->where(['mobile' => $mobile, 'event_id' => $eid])->find();
        $this->checkDb($ret);
        if (!empty($ret)) {
            $this->result(Constant::ERROR_USER_ALREADY_REGISTER);
        }

        $ret = $userModel->where(['id' => $uid])->setField(['mobile' => $mobile, 
                    'name' => $name,
                    'city' => $this->requestParameter('city'),
                    'address' => $this->requestParameter('address'),
                    'birthday' => $this->requestParameter('birthday'),
        ]);
        $this->checkDb($ret);

        $service = $this->_getService($eid);
        $service->onRegister($uid, $eid);

        session_regenerate_id();
        $this->userLogin([
            'id' => $uid,
            'account' => $mobile,
            'nickname' => $mobile,
        ]);

        $this->addlog("wechatregister", $eid, $mobile);
        $this->OK();
    }

    public function login()
    {
        $mobile = $this->requestParameter("mobile");
        $event_id = $this->requestParameter("event_id");
        $code = $this->requestParameter("code");

        if (!$this->checkCode($mobile, $code)) {
            $this->result(Constant::ERROR_USER_SMS);
        }

        $eventModel = $this->getModel("events");
        $event = $eventModel->where(['id' => $event_id])->find();
        $this->checkDb($event);
        if (empty($event)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        $userModel = $this->getModel("users");
        $user = $userModel->where(['mobile' => $mobile])->find();
        $this->checkDb($user);

        if (empty($user)) {
            $ret = $userModel->add([
                'create_ts' => Utils::now(), 
                'mobile' => $mobile,
                'event_id' => $event_id,
                'name' => $this->requestParameter('name'),
                'city' => $this->requestParameter('city'),
                'address' => $this->requestParameter('address'),
                'birthday' => $this->requestParameter('birthday'),
            ]);
            $this->checkDb($ret);
            $uid = $ret;

            $service = $this->_getService($event_id);
            $service->onRegister($uid, $event_id);
        } else {
            $uid = $user['id'];
        }

        $this->userLogin([
            'id' => $uid,
            'account' => $mobile,
            'nickname' => $mobile,
        ]);

        $this->addlog("login", $event_id, $mobile);
        $this->OK();
    }

    public function register() 
    {
        $this->checkLogin();
        $digest = $this->requestParameter("digest");
        $uid = $this->currentUid();

        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $service = $this->_getService($eid);

        $userModel = $this->getModel("users");
        $ret = $userModel->where(['id' => $uid])->setField(['digest' => $digest]);
        $this->checkDb($ret);

        $service->onRegisterDetail($uid, $eid);

        $this->OK();
    }

    public function logout() 
    {
        $this->userLogout();
        $this->OK();
    }

    private function checkApi($eid, $key)
    {
        $event = $this->getEvent($eid);
        if ($event['api_key'] !== $key) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
    }

    public function info()
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();
        $user = $this->getUser($uid);
        $eid = $user['event_id'];

        $useritemModel = $this->getModel('user_items');
        $items = $useritemModel->where([
            'user_id' => $uid,
            'event_id' => $eid,
        ])->select();
        $this->checkDb($items);

        $this->addlog('info', $eid);
        $this->OK([ 'user' => $user, 'items' => $items]);
    }

    public function detail()
    {
        $this->checkLogin();
        $this->checkAccount();
        $code = $this->requestParameter("code");
        $uid = $this->currentUid();
        $user = $this->getUser($uid);

        $eid = $user['event_id'];
        $event = $this->getEvent($eid);
        $itemcfg = $this->getItem($eid, $code);
        $id = $itemcfg['id'];

        $useritemModel = $this->getModel('user_event_items');
        $item = $useritemModel->where([
            'user_id' => $uid,
            'event_id' => $eid,
            'item_id' => $id
        ])->find();
        $this->checkDb($item);

        $this->addlog('detail', $eid, $id);
        $this->OK($item);
    }

    public function enjoy()
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();

        $code = $this->requestParameter('code');
        $content = urldecode($this->requestParameter('content'));
        $item = $this->_enjoy($uid, $code, $content);
        $this->OK($item);
    }

    public function enjoy2()
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();

        $code = $this->requestParameter('code');
        $uploads = $this->uploadFiles();
        $content = $uploads['urls'][0];
        $item = $this->_enjoy($uid, $code, $content);
        $this->OK($item);
    }

    public function ticket()
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();
        $code = $this->requestParameter('code');
        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $event = $this->getEvent($eid);
        $itemcfg = $this->getItem($eid, $code);

        $id = $itemcfg['id'];
        $useritemModel = $this->getModel('user_event_items');
        $cond = [
            'user_id' => $uid,
            'event_id' => $eid,
            'item_id' => $id
        ];
        $item = $useritemModel->where($cond)->find();
        $this->checkDb($item);

        if (empty($item)) {
            $this->result(Constant::ERROR_USER_ITEMS_NOT_MATCH);
        }

        if ($item['status'] != Constant::STATUS_INIT) {
            $this->result(Constant::ERROR_USER_ITEM_STATUS_WRONG);
        }

        $code = Utils::gen_parameter($id, $uid);

        $service = $this->getService($event['service_class']);
        $url = $service->getTicketUrl($event['ticket_url'], $code);

        $this->addlog("ticket", $eid, $id, $url);
        $this->OK(['code' => $this->genQrImg($url)]);
    }

    public function settle_ticket() 
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();
        $url = $uid;
        $this->OK(['code' => $this->genQrImg($url)]);
    }

    public function play()
    {
        $this->checkLogin();
        $this->checkAccount();
        $code = $this->requestParameter('code');
        $uid = $this->currentUid();
        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $itemcfg = $this->getItem($eid, $code);

        if ($itemcfg['secure'] == Constant::FLAG_YES) {
            $this->result(Constant::ERROR_PERM_DENY);
        }

        $item_id = $itemcfg['id'];

        $this->_play($uid, $eid, $item_id);
        $this->OK();
    }

    public function custom_play() {
        $key = $this->requestParameter('key');
        $code = $this->requestParameter('code');

        if (empty($code)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $data = Utils::get_parameter($code);
        if (empty($data)) {
            $this->result(Constant::ERROR_PERM_DENY);
        }

        $uid = $data['uid'];
        $item_id = $data['id'];
        $user = $this->getUser($uid);
        $eid = $user['event_id'];

        $this->checkApi($eid, $key);

        $this->_play($uid, $eid, $item_id);
        $this->OK();
    }

    public function settle()
    {
        $this->checkLogin();
        $this->checkAccount();
        $uid = $this->currentUid();
        $this->_settle($uid);
        $this->OK();
    }

    public function complete_item()
    {
        $key = $this->requestParameter('key');
        $eid = $this->requestParameter('id');
        $code = $this->requestParameter('code');
        $status = $this->requestParameter('status');

        $itemcfg = $this->getItem($eid, $code);
        $item_id = $itemcfg['id'];

        $this->checkApi($eid, $key);

        if ($itemcfg['allow_push'] == Constant::FLAG_NO) {
            $this->result(Constant::ERROR_NOT_SUPPORT);
        }

        $useritemModel = $this->getModel('user_event_items');
        $useritemModel->startTrans();
        $item = $useritemModel->lock(true)->where([
            'item_id' => $item_id,
            'event_id' => $eid,
            'pstatus' => '0',
            'status' => Constant::STATUS_DONE,
        ])->find();
        $this->checkDb($item, function() use($useritemModel) {
            $useritemModel->rollback();
        });
        
        if (!empty($status) && !empty($item)) {
            $ret = $useritemModel->where([
                'id' => $item['id'],
            ])->setField(['pstatus' => '1']);
            $this->checkDb($ret, function() use($useritemModel) {
                $useritemModel->rollback();
            });
        }
        $useritemModel->commit();

        if (!empty($item)) {
            $service = $this->_getService($eid);
            $item['content'] = $service->filterItemContent($item['user_id'], $eid, $item_id, $code, $item['content']);
        }

        $this->OK($item);
    }

    public function my_item() {
        $this->checkLogin();
        $this->checkAccount();
        $code = $this->requestParameter('code');
        $uid = $this->currentUid();
        $user = $this->getUser($uid);
        $eid = $user['event_id'];
        $event = $this->getEvent($eid);
        $itemcfg = $this->getItem($eid, $code);
        $item_id = $itemcfg['id'];

        if ($itemcfg['allow_push'] == Constant::FLAG_NO) {
            $this->result(Constant::ERROR_NOT_SUPPORT);
        }

        $useritemModel = $this->getModel('user_event_items');
        $items = $useritemModel->where([
            'item_id' => $item_id,
            'event_id' => $eid,
            'pstatus' => '0',
            'status' => Constant::STATUS_DONE,
        ])->select();
        $this->checkDb($items);
        
        $index = array_search($uid, array_column($items, 'user_id'));
        
        if ($index === false) {
            $index = -1;
            //$this->result(Constant::ERROR_USER_NOT_IN_QUEUE);
        }

        $this->addlog("myitem", $eid, $item_id, $index);
        $this->OK(['index'=>$index]);
    }

    public function now() {
        $this->OK(['datetime' => Utils::now()]);
    }

    public function test() {
        //echo Utils::getDate("4/7/2019");
        //echo $content = urldecode($this->requestParameter('content'));
        //var_dump($this->_getService(2));
    }
}

