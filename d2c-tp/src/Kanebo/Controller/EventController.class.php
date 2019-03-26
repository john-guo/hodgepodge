<?php
namespace Kanebo\Controller;

use \Common\Common\Constant;
use \Common\Common\Utils;

class EventController extends \Common\Controller\WechatController {

		protected function getAppConfig($event) {
				return array(
			self::APPID => "wx00a452fdd31df373",
			self::APPSECRET => "61ad699c4eafc0f8d863b746a03e7fc7",
			self::AESKEY => "6Jf71AqsHEb4XwqIVePswu9hfHAe7Gm4cvE3q7SHrSb",
			self::TOKEN => "d2c",
			);
        }
        
        const GATEWAY_PATH = "/kanebo/event/gateway";
        const GAME1 = "game1";
        const GAME2 = "game2";
        static $GAME_URLS = 
        [
            self::GAME1 => "http://n.d2c-china.cn/event/jialibao/game1/index.html",
            self::GAME2 => "http://n.d2c-china.cn/event/jialibao/game2/index.html"
        ];

        public function game1() {
            $redirectUrl = $this->makeSiteUrl(self::GATEWAY_PATH);
            $url = $this->userAuthorizeUrl($redirectUrl, self::GAME1);
            if ($url === false)
                return;
            $this->jump($url);
        }

        public function game2() {
            $redirectUrl = $this->makeSiteUrl(self::GATEWAY_PATH);
            $url = $this->userAuthorizeUrl($redirectUrl, self::GAME2);
            if ($url === false)
                return;
            $this->jump($url);
        }

        public function gateway() {
            $state = $this->retrieveState();
            $url = self::$GAME_URLS[$state];
            if (empty($url)) {
                return;
            }

            $user = $this->retrieveUserInfo();
            if (empty($user))
                return;
    
            $open_id = $user['openid'];
            $nickname = $user['nickname'];
            $headimgurl = $user['headimgurl'];

            if (empty($open_id))
                return;
    
            $userModel = $this->getModel("users");
            $user =  $userModel->where(array("open_id" => $open_id))->find();
            $this->checkDb($user);
    
            if (empty($user)) {
                $uid = $userModel->add(array("open_id" => $open_id, "create_ts" => Utils::now(), "nickname" => $nickname, "header" => $headimgurl));
                $this->checkDb($uid);
                $this->addlog("register", $open_id, $uid);
            } else {
                $data = [];
                if ($user['nickname'] != $nickname) {
                    $data["nickname"] = $nickname; 
                }
                if ($user['header'] != $headimgurl) {
                    $data['header'] = $headimgurl;
                }
                $ret = $userModel->where(array("open_id" => $open_id))->setField($data);
                $this->checkDb($ret);
                $uid = $user['id'];
            }
    
            $this->userLogin(array('id' => $uid));
            $this->addlog("login", $open_id);
    
            $this->jump($url);
        }

        private function score($gameType) {
            $this->checkLogin();
            $uid = $this->currentUid();

            $score = $this->requestParameter("score");
            if (empty($score)) {
                return $this->result(Constant::ERROR_INVALID_PARAMETERS);
            }

            $cfg_key = "game_$gameType";
            $cfg = $this->getModel("configs")->where(['cfg_key' => $cfg_key])->find();
            $this->checkDb($cfg);
            if (!empty($cfg) && !empty($cfg['cfg_value'])) {
                return $this->result(Constant::ERROR_TIMEOUT);
            }

            $score_field = "score_$gameType";
            $scoretime_field = "score_time_$gameType";
            $now = Utils::now();
            $userModel = $this->getModel("users");
            $ret = $userModel->where(array("id" => $uid, $score_field => ['lt', $score] ))->setField([$score_field => $score, $scoretime_field => $now]);
            $this->checkDb($ret);

            return $this->OK();
        }

        private function top($gameType) {
            $userModel = $this->getModel("users");
            $score = "score_$gameType";
            $scoretime_field = "score_time_$gameType";
            $ret = $userModel->where([$score =>  ['gt', 0]])->order([$score => "desc", $scoretime_field => "asc"])->limit(10)->select();
            $this->checkDB($ret);
            return $this->OK($ret);
        }

        private function stop($gameType) {
            $stop = $this->requestParameter("stop");
            $cfg_key = "game_$gameType";
            $cfg = $this->getModel("configs")->where(['cfg_key' => $cfg_key])->setField(["cfg_value" => "$stop"]);
            $this->checkDb($cfg);
            return $this->OK();
        }

        public function score1() {
            return $this->score(1);
        }

        public function score2() {
            return $this->score(2);
        }

        public function top1() {
            return $this->top(1);
        }

        public function top2() {
            return $this->top(2);
        }

        public function stop1() {
            return $this->stop(1);
        }

        public function stop2() {
            return $this->stop(2);
        }

        public function stat() {
            $cfg = $this->getModel("configs")->select();
            $this->checkDb($cfg);

            foreach ($cfg as $item) {
                echo $item['cfg_key'] . " : " . (empty($item['cfg_value']) ? "Open" : "Close") . "<br/>";
            }
        }
}