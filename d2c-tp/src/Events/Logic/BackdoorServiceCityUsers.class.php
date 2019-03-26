<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IBackdoor;

class BackdoorServiceCityUsers extends \Common\Controller\BackdoorController implements IBackdoor
{
    public function play($event_id, $item_id, $uid) {
		// $nowtime = date("H:i:s");
		// if ($nowtime < EventController::EVENT_DAY_OPENTIME || 
		// 		$nowtime > EventController::EVENT_DAY_CLOSETIME) {
		// 	$this->addbackdoorlog("error", "play", "out of play time");
		// 	$this->backdoor_FAIL();
		// }

		// $cond = array('id' => $uid);
		// $userModel = M("users");
		// $ret = $userModel->where($cond)->find();
		// $this->checkDb($ret);
		// if (empty($ret)) {
		// 	$this->addbackdoorlog("error", "play", "user not found");
		// 	$this->backdoor_FAIL();
		// }

		// $city = $ret['city'];
		// $reward_time = $ret['reward_time'];
		// if (!empty($reward_time)) {
		// 	$this->addbackdoorlog("error", "play", "already reward:$event");
		// 	$this->backdoor_FAIL();
		// }

		// $now = date('Y-m-d H:i:s');
		// $opentime = constant("\Kose\Controller\EventController::EVENT_CITY_DATE_$city");
		// $closetime = constant("\Kose\Controller\EventController::EVENT_CITY_DATE_END_$city");

		// $diff1 = $this->isExpired($now, $opentime);
		// $diff2 = $this->isExpired($now, $closetime);

		// if (!$diff1 || $diff2) {
		// 	$this->addbackdoorlog("error", "play", "out of event time");
		// 	$this->backdoor_FAIL();
		// }

		// $data = array();
		// $data['code'] = '';

		// $ret = $userModel->where($cond)->setField(array("event_$event" => 1));
		// $this->checkDb($ret);
		// if (empty($ret)) {
		// 	$this->addbackdoorlog("error", "play", "already play:$event");
		// 	$this->backdoor_FAIL();
		// }

		// $ret = $userModel->where($cond)->find();
		// $this->checkDb($ret);

		// $reward = true;
		// for ($i = EventController::EVENT_1; $i <= EventController::EVENT_5; $i++) {
		// 	if ($ret["event_$i"] != 1) {
		// 		$reward = false;
		// 		break;
		// 	}
		// }
		
		// if ($reward) {
		// 	$code = substr(str_shuffle("0123456789"), 0, 3);
		// 	$reward_time = date_add(new \DateTime($now), new \DateInterval("PT24H"));
		// 	$ret = $userModel->where($cond)->setField(array("reward_no" => $code, "reward_time" => $reward_time->format('Y-m-d 23:59:59')));
		// 	$this->checkDb($ret);	
		// 	$data['code'] = $code;
		// 	$this->addlog($uid, 'homerun', $event);
			
		// 	$this->addbackdoorlog("homerun", $uid, $event);
		// }

		// $this->addlog($uid, 'play', $event);

		// $this->addbackdoorlog("play", $uid, $event);

		// $this->OK($data);
    }
}