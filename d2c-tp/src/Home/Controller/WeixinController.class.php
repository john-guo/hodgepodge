<?php
namespace Home\Controller;

use \Common\Common\Constant;
use \Common\Common\Utils;

class WeixinController extends \Freeplus\Controller\EventController {
			
		protected function getAppConfig($event) {
				return array(
			self::APPID => "wx00a452fdd31df373",
			self::APPSECRET => "61ad699c4eafc0f8d863b746a03e7fc7",
			self::AESKEY => "6Jf71AqsHEb4XwqIVePswu9hfHAe7Gm4cvE3q7SHrSb",
			self::TOKEN => "d2c",
			);
		}
}