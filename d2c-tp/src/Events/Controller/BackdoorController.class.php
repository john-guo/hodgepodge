<?php
namespace Events\Controller;

use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\AdminBaseController;

class BackdoorController extends AdminBaseController {

	const BACKDOOR_PASSWORD = "backdoor";
	const BACKDOOR_NAME_CNAME = 'eman';
	const BACKDOOR_PASSWORD_CNAME = "drowssap";
	const BACKDOOR_EVENT_CNAME = "tneve";
	const BACKDOOR_ITEM_CNAME = "emti";
	const BACKDOOR_SIGN_CNAME = 'ngis';
	const BACKDOOR_TIMEOUT = 31536000;
	const IP_MAX_TRY_COUNT = 5;
	const IP_LIMITED_TIMEOUT = 60;

	protected function addbackdoorlog($type, $target='', $memo='', $operator = '') {
		$log = M("backdoor_logs");

		$data = array();
		$data['operator'] = empty($operator) ? $_COOKIE[self::BACKDOOR_NAME_CNAME] : $operator;
		$data['type'] = $type;
		$data['memo'] = $memo;
		$data['target'] = $target;
		$data['ip'] = get_client_ip();
		
		$log->add($data);
	}

	private function checkbackdoor_login() {
		if (!$this->isbackdoor_login()) {
			$this->addbackdoorlog("error", "checklogin");
			$this->backdoor_FAIL();
		}
	}

	private function isbackdoor_login() {
		$name = $_COOKIE[self::BACKDOOR_NAME_CNAME];
		$password = $_COOKIE[self::BACKDOOR_PASSWORD_CNAME];

		if (empty($name) && empty($password)) {
			return false;
		}

		if (empty($name) || empty($password) || md5(self::BACKDOOR_PASSWORD) !== $password) {

			if (!empty($name) && !empty($password)) {
				$this->backdoor_logout();
			}

			return false;
		}

		return true;
	}

	private function setbackdoorcookie($name, $value, $timeout = self::BACKDOOR_TIMEOUT) {
		\setcookie($name, $value, time() + $timeout, '', '', false, true);// \strtolower(\parse_url(EventController::HOST, PHP_URL_SCHEME)) == "https", true);
	}

	private function backdoor_login($name, $password) {
		$this->setbackdoorcookie(self::BACKDOOR_NAME_CNAME, $name);
		$this->setbackdoorcookie(self::BACKDOOR_PASSWORD_CNAME, md5($password));
	}

	private function backdoor_logout() {
		$this->setbackdoorcookie(self::BACKDOOR_NAME_CNAME, '', -self::BACKDOOR_TIMEOUT);
		$this->setbackdoorcookie(self::BACKDOOR_PASSWORD_CNAME, '', -self::BACKDOOR_TIMEOUT);
		$this->setbackdoorcookie(self::BACKDOOR_EVENT_CNAME, '', -self::BACKDOOR_TIMEOUT);
		$this->setbackdoorcookie(self::BACKDOOR_ITEM_CNAME, '', -self::BACKDOOR_TIMEOUT);
		$this->setbackdoorcookie(self::BACKDOOR_SIGN_CNAME, '', -self::BACKDOOR_TIMEOUT);
	}

	protected function backdoor_FAIL() {
		$this->result(-1, "");
	}

	private function makesign($name, $password, $event_id, $item_id) {
		return md5("$item_id|$event_id|$name|$password|".self::BACKDOOR_PASSWORD);
	}

	private function sign($event_id, $item_id) {
		$name = $_COOKIE[self::BACKDOOR_NAME_CNAME];
		$password = $_COOKIE[self::BACKDOOR_PASSWORD_CNAME];
		$sign = $this->makesign($name, $password, $event_id, $item_id);
		$this->setbackdoorcookie(self::BACKDOOR_SIGN_CNAME, $sign);
	}

	private function issign() {
		$name = $_COOKIE[self::BACKDOOR_NAME_CNAME];
		$password = $_COOKIE[self::BACKDOOR_PASSWORD_CNAME];
		$event_id = $_COOKIE[self::BACKDOOR_EVENT_CNAME];
		$item_id = $_COOKIE[self::BACKDOOR_ITEM_CNAME];
		$sign = $_COOKIE[self::BACKDOOR_SIGN_CNAME];

		return $this->makesign($name, $password, $event_id, $item_id) == $sign;
	}

	private function checksign() {
		if (!$this->issign()) {
			$this->addbackdoorlog("error", "checksign");
			$this->backdoor_FAIL();
		}
	}

	public function login() {
		$name = I("name");
		$password = \urldecode(I("password"));
		$phrase = I("phrase");

		if (empty($phrase) || $phrase != $_SESSION['phrase']){
			$this->backdoor_FAIL();
		}

		if (empty($name)) {
			$this->backdoor_FAIL();
		}

		$redis = new \Redis();
		$redis->pconnect("localhost");
		$ip = get_client_ip();
		$count = $redis->get($ip);
		if (!empty($count) && $count > self::IP_MAX_TRY_COUNT) {
			$redis->setEx($ip, self::IP_LIMITED_TIMEOUT, $count);
			$this->backdoor_FAIL();	
		}

		if ($password != self::BACKDOOR_PASSWORD) {
			$this->addbackdoorlog("error", "loginfail", '', $name);
			if (empty($count)) {
				$count = 1;
			} else {
				$count++;
			}
			$redis->setEx($ip, self::IP_LIMITED_TIMEOUT, $count);
			$redis->close();

			$this->backdoor_FAIL();			
		}

		$redis->close();
		$this->backdoor_login($name, $password);

		$this->addbackdoorlog("login", $name, '', $name);

		$this->OK();
	}

	public function install() {
		$this->checkbackdoor_login();
		$event_id = $this->requestParameter("event_id");
		$item_id = $this->requestParameter("item_id");

		if (empty($event_id) || !\is_numeric($event_id)) {
			$this->backdoor_FAIL();
		}

		if (empty($item_id) || !\is_numeric($item_id)) {
			$this->backdoor_FAIL();
		}

		$this->setbackdoorcookie(self::BACKDOOR_EVENT_CNAME, $event_id);
		$this->setbackdoorcookie(self::BACKDOOR_ITEM_CNAME, $item_id);

		$this->sign($event_id, $item_id);

		$this->addbackdoorlog("install", $event_id, $item_id);

		$this->OK();
	}

	private function isExpired($time, $targetTime)
	{
		$diff = date_diff(new \DateTime($time), new \DateTime($targetTime));
		if ($diff->invert === 1) {
			return true;
		}
		return false;
	}

	public function scan() {
		$this->checkbackdoor_login();
		$this->checksign();

		$event_id = $_COOKIE[self::BACKDOOR_EVENT_CNAME];
		$item_id = $_COOKIE[self::BACKDOOR_ITEM_CNAME];
		if (empty($event_id) || !\is_numeric($event_id)) {
			$this->addbackdoorlog("error", "scan", "event_id wrong:$event_id");
			$this->backdoor_FAIL();
		}

		if (empty($item_id) || !\is_numeric($item_id)) {
			$this->addbackdoorlog("error", "scan", "item_id wrong:$item_id");
			$this->backdoor_FAIL();
		}

		$code = $this->requestParameter("code");
		$params = Utils::get_parameter($code);
		if (empty($params)) {
			$this->addbackdoorlog("error", "scan", "code wrong:$code");
			$this->backdoor_FAIL();
		}

		$uid = $params['uid'];
		$id = $params['id'];
		if ($id != $item_id) {
			$this->addbackdoorlog("error", "scan", "id notmatch:$id != $item_id");
			$this->backdoor_FAIL();
		}

		$this->addbackdoorlog("scan", $uid, $event_id);

		$eventModel = $this->getModel("events");
		$targetEvent = $eventModel->where(['id' => $event_id])->find();
		$this->checkDb($targetEvent);
		if (empty($targetEvent)) {
			$this->result(Constant::ERROR_RECORD_NOTFOUND);
		}
		$class_name = $targetEvent['class_name'];
		if (empty($class_name)) {
			$class_name = "\Events\Logic\DefaultBackdoorService";
		}

		$target = new $class_name;
		$target->play($event_id, $item_id, $uid);
	}

	public function info() {
		$this->addbackdoorlog("info");

		$data = array();
		if (!$this->isbackdoor_login()) {
			$data['msg'] = 'not login';
			$data['installed'] = false;
			$this->OK($data);
		}
		if (!$this->issign()) {
			$data['msg'] = 'sign error';
			$data['installed'] = false;
			$this->OK($data);
		}

		$event_id = $_COOKIE[self::BACKDOOR_EVENT_CNAME];
		if (empty($event_id) || !\is_numeric($event_id)) {
			$data['msg'] = 'event error';
			$data['installed'] = false;
			$this->OK($data);
		}

		$item_id = $_COOKIE[self::BACKDOOR_ITEM_CNAME];
		if (empty($item_id) || !\is_numeric($item_id)) {
			$data['msg'] = 'item error';
			$data['installed'] = false;
			$this->OK($data);
		}

		$data['installed'] = true;
		$data['event_id'] = $event_id;
		$data['item_id'] = $item_id;
		$this->OK($data);
	 }
	 
	 public function uninstall() {
		$this->addbackdoorlog("uninstall");

		$this->backdoor_logout();
		$this->OK();
	 }
}