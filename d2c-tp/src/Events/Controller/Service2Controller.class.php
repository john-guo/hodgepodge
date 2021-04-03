<?php
namespace Events\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IWechat;

class Service2Controller extends \Common\Controller\WechatController 
{
    use \Common\Controller\WechatOperation;
    use \Events\Logic\EventService;
    private $iwechats; 
    const GATEWAY_PATH = "/events/service2/gateway/id/";
    const GATEWAY_PATH2 = "/events/service2/gateway2/id/";
    const USERINFO_TIMEOUT = 300;

    function __construct() {
        parent::__construct();

        $this->currentEvent = $this->requestParameter("path.3");
        $this->iwechats = null;

        $wechatsModel = $this->getModel("event_wechats");
        $item = $wechatsModel->where(['id' => $this->currentEvent])->find();
        if ($item == null) {
            $this->weixinlog('event_wechats not found', "error", $this->currentEvent, '', '');
            return;
        }
        $obj = null;
        if (!empty($item['class_name'])) {
            $obj = new $item['class_name']($item['id']);
        } else {
            $obj = new \Events\Logic\DefaultWechatService(0);
        }
        $this->iwechats = [ 'cfg' => $item, 'obj' => $obj ];
    }

    protected function getAppConfig($event) {

        $cfg = $this->getWechatConfig();

        if ($cfg == null)
            return null;

        return array(
			self::APPID => $cfg['appid'],
			self::APPSECRET => $cfg['appsecret'],
			self::AESKEY => $cfg['aeskey'],
			self::TOKEN => $cfg['token'],
    	);
    }

    private function getWechatConfig() {
        if ($this->iwechats == null) {
            return null;
        }
        return $this->iwechats['cfg'];
    }

    private function getIWechat() {
        if ($this->iwechats == null)
            return null;
        return $this->iwechats['obj'];
    }

    protected function getScanResult($user, $code) {
        $obj = $this->getIWechat();
        if ($obj == null) {
            return parent::getScanResult($user, $code);
        }

        return $obj->onUserScan($user, $code);
	}

	protected function autoreplyrule() {
        $obj = $this->getIWechat();
        if ($obj == null) {
            return parent::autoreplyrule();
        }

        return $obj->getAutoReplyConfig();
	}

	protected function menuClick($key) {
        $obj = $this->getIWechat();
        if ($obj == null) {
            return parent::menuClick($key);
        }

        return $obj->onMenuClick($key);
	}

	protected function greeting_msg($user) {
        $obj = $this->getIWechat();
        if ($obj == null) {
            return parent::greeting_msg($user);
        }

        return $obj->getGreetingMsg($user);
	}

	protected function menuJson() {
        $obj = $this->getIWechat();
        if ($obj == null) {
            return parent::menuJson();
        }

        return $obj->getMenuConfig();
    }
    
    /*
    common
    */
    public function wechatlogin() 
    {
        $redirectUrl = $this->makeSiteUrl(self::GATEWAY_PATH . $this->currentEvent);
        $url = $this->baseAuthorizeUrl($redirectUrl);
        if ($url === false)
            return;
        $this->jump($url);
    }

    public function wechatlogin2() {
        $redirectUrl = $this->makeSiteUrl(self::GATEWAY_PATH2 . $this->currentEvent);
        $url = $this->userAuthorizeUrl($redirectUrl);
        if ($url === false)
            return;
        $this->jump($url);
    }

    public function miniproglogin() {
        $code = $this->requestParameter("code");
        if (empty($code)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $mini_id = $this->requestParameter('mini_id');
        $miniModel = $this->getModel("event_miniprogs");
        $cond = ['event_id' => $this->currentEvent];
        if (!empty($mini_id)) {
            $cond += ['id' => $mini_id];
        }
        $minicfg = $miniModel->where($cond)->find();
        $this->checkDb($minicfg);

        if (empty($minicfg)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        $data = $this->getMiniProgramSession($minicfg['app_id'], $minicfg['appsecret'], $code);
        if ($data === false) {
            $this->result(Constant::ERROR_FAILED);
        }

        $this->OK($data);
    }

    public function gateway() 
    {
        $open_id = $this->retrieveOpenId();
        if (empty($open_id))
            return;

        $cfg = $this->getWechatConfig();
        $code = Utils::gen_parameter(0, $open_id);
        $this->jump($cfg['boot_url'] . "?openid=$code");
    }

    public function gateway2() 
    {
        $userinfo = $this->retrieveUserInfo();
        if (empty($userinfo))
            return;

        $openid = $userinfo['openid'];
        $this->map_store($openid, json_encode($userinfo), self::USERINFO_TIMEOUT);

        $cfg = $this->getWechatConfig();
        $code = Utils::gen_parameter(0, $openid);
        $this->jump($cfg['boot_url'] . "?openid=$code");
    }

    public function wechatuserinfo() 
    {
        $code = $this->requestParameter("open_id");
        $data = Utils::get_parameter($code);
        if (empty($data)) {
            $this->result(Constant::ERROR_FAILED);
        }

        $json = $this->map_retrieve($data['uid']);
        $this->OK(json_decode($json, true));
    }

    public function ticket()
    {
        $key = $this->requestParameter('key');
        $content = $this->requestParameter('content');

        $event_id = $this->currentEvent;
        $cfg = $this->getEvent($event_id);
        if ($cfg['api_key'] !== $key) {
            $this->result(Constant::ERROR_PERM_DENY);
        }

        $ticketModel = $this->getModel("event_tickets");
        $ret = $ticketModel->add([
            'create_ts' => Utils::now(),
            'event_id' => $event_id,
            'content' => $content
        ]);
        $this->checkDb($ret);

        $url = $this->genQrUrl($ret);
        $this->OK(['content' => $url]);
    }

    public function jssdk() 
    {
        $data = $this->getJsSdkConfig();
        $this->OK($data);
    }

    /*admin*/
    private function checkAdmin() {
        $admin = new \Events\Controller\AdminController;
        if (!$admin->external_isAdmin()) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
    }

    public function createpublicticket() {
        $this->checkAdmin();
        $code = $this->requestParameter('code');
        if (empty($code))
            $code = Constant::DEFAULT_WECHAT_PUBLIC_TICKET;
        $url = $this->genQrUrl($code);
        $this->OK(['code' => $this->genQrImg($url)]);
    }

    public function createminiprogramticket() 
    {
        $path = $this->requestParameter('path');
        if (empty($path)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $mini_id = $this->requestParameter('mini_id');
        $miniModel = $this->getModel("event_miniprogs");
        $cond = ['event_id' => $this->currentEvent];
        if (!empty($mini_id)) {
            $cond += ['id' => $mini_id];
        }
        $minicfg = $miniModel->where($cond)->find();
        $this->checkDb($minicfg);

        if (empty($minicfg)) {
            $this->result(Constant::ERROR_RECORD_NOTFOUND);
        }

        $this->genMiniProgramQrCode($minicfg['app_id'], $minicfg['appsecret'], $path);
    }

}