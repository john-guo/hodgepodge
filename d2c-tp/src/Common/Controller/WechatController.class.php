<?php
namespace Common\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;

abstract class WechatController extends \Common\Controller\CommonController {

	const APPID = "APPID";
	const APPSECRET = "APPSECRET";
	const AESKEY = "AESKEY";
	const TOKEN = "TOKEN";

	const DEBUG = false;
	const API_RETRY_COUNT = 10;
	const API_RETRY_TIMEOUT = 5;

    protected $currentEvent = 0;
    protected function getAppConfig($event) {
        return array(
			self::APPID => "",
			self::APPSECRET => "",
			self::AESKEY => "",
			self::TOKEN => "",
    	);
    }

	private function textReply($to, $from, $time, $text) {
		return "<xml> 
		<ToUserName><![CDATA[$to]]></ToUserName>
		<FromUserName><![CDATA[$from]]></FromUserName>
		<CreateTime>$time</CreateTime>
		<MsgType><![CDATA[text]]></MsgType>
		<Content><![CDATA[$text]]></Content>
		</xml>";
	}

	private function imagesReply($to, $from, $time, $images) {
		$count = count($images);
		$text = "<xml>
		<ToUserName><![CDATA[$to]]></ToUserName>
		<FromUserName><![CDATA[$from]]></FromUserName>
		<CreateTime>$time</CreateTime>
		<MsgType><![CDATA[news]]></MsgType>
		<ArticleCount>$count</ArticleCount>
		<Articles>";
		foreach ($images as $img) {
			$title = $img['title'];
			$digest = $img['digest'];
			$cover_url = $img['cover_url'];
			$content_url = $img['content_url'];
			$text .= "<item>
				<Title><![CDATA[$title]]></Title>
				<Description><![CDATA[$digest]]></Description>
				<PicUrl><![CDATA[$cover_url]]></PicUrl>
				<Url><![CDATA[$content_url]]></Url>
				</item>";
		}
		$text .= "</Articles>
		</xml>";

		return $text;
	}

	private function imageReply($to, $from, $time, $title, $digest, $cover_url, $content_url) {
		return "<xml>
		<ToUserName><![CDATA[$to]]></ToUserName>
		<FromUserName><![CDATA[$from]]></FromUserName>
		<CreateTime>$time</CreateTime>
		<MsgType><![CDATA[news]]></MsgType>
		<ArticleCount>1</ArticleCount>
		<Articles>
			<item>
				<Title><![CDATA[$title]]></Title>
				<Description><![CDATA[$digest]]></Description>
				<PicUrl><![CDATA[$cover_url]]></PicUrl>
				<Url><![CDATA[$content_url]]></Url>
			</item>
		</Articles>
		</xml>";
	}

	protected function getAccessToken($update = false, $force = false) {
		$event = $this->currentEvent;
		$appConfig = $this->getAppConfig($event);
		$appId = $appConfig[self::APPID];
		$appSecret = $appConfig[self::APPSECRET];

        if (empty($appId) || empty($appSecret))
            return false;

		$key = "access_token_". $appId;
		$access_token = $this->weixinToken($key, "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=$appId&secret=$appSecret", "access_token", false, $update, $force);
		return $access_token;
	}

	protected function getJsApiTicket($update = false, $force = false) {
		$event = $this->currentEvent;
		$key = "jsapi_ticket_". $event;

		$jsapi_ticket = $this->weixinToken($key, "https://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token=", "ticket", true, $update, $force);
		return $jsapi_ticket;
	}

	protected function genQrUrl($code, $expire = 2592000) {
		if (empty($code))
			return false;

		$scene_str = $code;

		$data = array(
			'expire_seconds' => $expire,
			'action_name' => 'QR_STR_SCENE',
			'action_info' => array(
				'scene' => array(
					'scene_str' => $scene_str
				)
			)
		);
		$result = $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token=", true, json_encode($data));
		if (empty($result)) {
			return null;
		}
		//$ticket = urlencode($result['ticket']);
		//redirect("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=$ticket");
		return $result['url'];
	}

	protected function sendCustomMessage($messages) {
		$json = json_encode($messages, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
		$this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=", true, $json);
	}

	/* weixin utils */
	private function weixinToken($key, $url, $tokenName, $accessToken = false, $update = false, $force = false) {
		$redis = new \Redis();
		$redis->connect('127.0.0.1');
		$testKey = "test_" . $key;
		if ($force) {
			$redis->del($testKey);
		}

		$token = $redis->get($key);
		if ($update || empty($token))
		{
			$firstSet = $redis->set($testKey, time(),  array('nx', 'ex'=>static::API_RETRY_TIMEOUT));
			if (!$firstSet) {
				do {
					$now = time();
					$firstSet = $redis->set($testKey, $now,  array('nx', 'ex'=>static::API_RETRY_TIMEOUT));
					if ($firstSet) {
						$token = $redis->get($key);
						if (empty($token)) {
							break;
						} else {
							$redis->del($testKey);
							$redis->close();
							return $token;
						}
					}
				} while (!$firstSet);
			}

			$json = $this->weixinApiInvoke($url, $accessToken);
			$this->weixinlog("refresh token [$key] " . var_export($json, true) , "info", "update=$update,force=$force", $url, "accessToken=$accessToken");

			if (empty($json)) {
				$redis->del($testKey);
				$redis->close();
				return false;
			}
			$token = $json[$tokenName];
			if (empty($token)) {
				$redis->del($testKey);
				$redis->close();
				return false;
			}
			$expire = $json['expires_in'];
			if ($expire <= 0) {
				$redis->del($testKey);
				$redis->close();
				return false;
			}
			$redis->setEx($key, $expire, $token);
			$redis->del($testKey);
		}
		$redis->close();

		return $token;
	}


	protected function weixinApiInvoke($apiUrl, $accessToken = false, $postJson = null, $raw = false) {
		$tryCount = 0;
		$url = '';
		do {
			$result = null;
			$url = $apiUrl;
			if ($accessToken) {
				$access_token = $this->getAccessToken();
				if ($access_token === false)
					return false;
				$url .= $access_token;
			}
			if (!empty($postJson)) {
				$result = Utils::curlJsonPost($url, $postJson);
			} else {
				$result = Utils::curlJsonGet($url);
			}

			if ($raw)
				return $result;

			if (empty($result)) {
				$this->weixinlog("empty result", "error", 'fatal error', $url, '');
				continue;
			}

			$json = json_decode($result, true);
			if (empty($json)) {
				$this->weixinlog($result, "error", 'json error', $url, '');
				continue;
			}

			$errcode = $json['errcode'];
			if (empty($errcode)) {
				if (static::DEBUG) {
					$this->weixinlog($result, "info", 'json result', $url, '');
				}
				return $json;
			}

			if ($errcode == -1) {
				continue;
			}

			$this->weixinlog($result, "error", 'api error', $url, '');

			if ($accessToken && $errcode == 40001) {
				$access_token = $this->getAccessToken(true);
				if (!empty($access_token)) {
					$tryCount = 0;
					continue;
				}
			}
			return null;
		} while ($tryCount++ < static::API_RETRY_COUNT);

		$this->weixinlog($result, "error", 'out of try count', $url, '');
		return null;
	}

	protected function weixinlog($memo, $type, $event, $from, $to) {
		$log = $this->getModel("weixin_logs");
		$data = array();
		$data['action'] = $this->currentEvent;
		$data['memo'] = $memo;
		$data['type'] = $type;
		$data['event'] = $event;
		$data['from'] = $from;
		$data['to'] = $to;
		$log->add($data);
	}

	private function weixinSha1($ar, $delimiter = '') {
		sort($ar, SORT_STRING);
		$result = implode($delimiter, $ar);
		return sha1($result);
	}

	private function weixinEncrypt($key, $text, $appid) {
		$random = openssl_random_pseudo_bytes(16);
		$text = $random . pack("N", strlen($text)) . $text . $appid;
		$module = mcrypt_module_open(MCRYPT_RIJNDAEL_128, '', MCRYPT_MODE_CBC, '');
		$iv = substr($key, 0, 16);
		$blocksize = 32;
		$len = strlen($text);
		$padding = $blocksize - ($len % $blocksize);
		if ($padding == 0)
			$padding = $blocksize;
		$text .= str_repeat(chr($padding), $padding);
		mcrypt_generic_init($module, $key, $iv);
		$encrypted = mcrypt_generic($module, $text);
		mcrypt_generic_deinit($module);
		mcrypt_module_close($module);
		return base64_encode($encrypted);
	}

	private function weixinDecrypt($key, $encrypted, $appid) {
		$ciphertext_dec = base64_decode($encrypted);
		$module = mcrypt_module_open(MCRYPT_RIJNDAEL_128, '', MCRYPT_MODE_CBC, '');
		$iv = substr($key, 0, 16);
		mcrypt_generic_init($module, $key, $iv);
		$decrypted = mdecrypt_generic($module, $ciphertext_dec);
		mcrypt_generic_deinit($module);
		mcrypt_module_close($module);
		$padding = ord(substr($decrypted, -1));
		if ($padding < 0 || $padding > 32)
			$padding = 32;
		$result = substr($decrypted, 0, strlen($decrypted) - $padding);
		if (strlen($result) < 16) {
			return false;
		}
		$content = substr($result, 16, strlen($result));
		$len_list = unpack("N", substr($content, 0, 4));
		$xml_len = $len_list[1];
		$xml_content = substr($content, 4, $xml_len);
		$from_appid = substr($content, $xml_len + 4);
		if ($from_appid != $appid) {
			return false;
		}
		return $xml_content;
	}

	private function weixinDecryptMsg($key, $token, $appid, $msg_sign, $content, $timestamp, $nonce) {
		if (strlen($key) != 43) {
			return false;
		}
		$key = base64_decode($key . "=");
		libxml_disable_entity_loader(true);
		$xml = new \DOMDocument();
		$xml->loadXML($content);
		$encrypt = $xml->getElementsByTagName('Encrypt')->item(0)->nodeValue;
		$tousername = $xml->getElementsByTagName('ToUserName')->item(0)->nodeValue;
		$hash = $this->weixinSha1([$encrypt, $token, $timestamp, $nonce]);
		if ($hash != $msg_sign) {
			return false;
		}
		return $this->weixinDecrypt($key, $encrypt, $appid);
	}

	private function weixinEncryptMsg($key, $token, $appid, $content, $timestamp, $nonce) {
		if (strlen($key) != 43)
			return false;
		$key = base64_decode($key . "=");
		$encrypt = $this->weixinEncrypt($key, $content, $appid);
		$hash = $this->weixinSha1([$encrypt, $token, $timestamp, $nonce]);
		$format = "<xml>
<Encrypt><![CDATA[%s]]></Encrypt>
<MsgSignature><![CDATA[%s]]></MsgSignature>
<TimeStamp>%s</TimeStamp>
<Nonce><![CDATA[%s]]></Nonce>
</xml>";
		return sprintf($format, $encrypt, $hash, $timestamp, $nonce);
	}

	protected function decryptMsg($cfg, $contents, $msg_sign, $timestamp, $nonce) {
		$result = $this->weixinDecryptMsg($cfg[self::AESKEY], $cfg[self::TOKEN], $cfg[self::APPID], $msg_sign, $contents, $timestamp, $nonce);
		if ($result === false)
			exit;
		return $result;
	}

	protected function encryptMsg($cfg, $contents, $timestamp, $nonce) {
		$result = $this->weixinEncryptMsg($cfg[self::AESKEY], $cfg[self::TOKEN], $cfg[self::APPID], $contents, $timestamp, $nonce);
		if ($result === false)
			exit;
		return $result;
	}

	protected function weixinCustomMessageText($user, $content) {
		return 
			array(
				"touser" => $user,
				"msgtype" => "text",
				"text" => array( "content" => $content )
			);
	}

	protected function weixinCustomMessageMedia($user, $media_id, $mediaType = "image") {
		return 
			array(
				"touser" => $user,
				"msgtype" => $mediaType,
				"$mediaType" => ["media_id" => $media_id] 
			);
	}

	protected function weixinCustomMessageNews($user, $title, $description, $pic_url, $url) {
		return 
			array(
				"touser" => $user,
				"msgtype" => "news",
				"news" => array(
					"articles" => array(
						array(
							"title" => $title,
							"description" => $description,
							"url" => $url,
							"picurl" => $pic_url
						)
					)
				) 
			);
	}
	/* end */

	protected function process() {
		$appConfig = $this->getAppConfig($this->currentEvent);
		$appId = $appConfig[self::APPID];
		$appSecret = $appConfig[self::APPSECRET];
		$encodingAesKey = $appConfig[self::AESKEY];
		$token = $appConfig[self::TOKEN];

		if (empty($appId) || empty($appSecret))
            return;

		if (IS_GET) {
			$signature = $_GET["signature"];
			$timestamp = $_GET["timestamp"];
			$nonce = $_GET["nonce"];
			$echostr = $_GET['echostr'];

			if (empty($signature)) {
				echo "Entry point";
			}

			$tmpArr = array($token, $timestamp, $nonce);
			$tmpStr = $this->weixinSha1($tmpArr);
		
			if( $tmpStr == $signature ){
				echo $echostr;
			}
			return;
		}

		if (empty($encodingAesKey) || empty($token)) {
            return;
        }

		$contents = file_get_contents("php://input");
		if (empty($contents)) {
			return;
		}

		$timestamp = $_GET["timestamp"];
		$nonce = $_GET["nonce"];
		$msg_sign = $_GET["msg_signature"];

		$msg = $this->decryptMsg($appConfig, $contents, $msg_sign, $timestamp, $nonce);
		if (empty($msg)) {
			return;
		}

		$doc = new \DOMDocument();
		$doc->loadXML($msg);
		$msgType = $doc->getElementsByTagName('MsgType')->item(0)->nodeValue;
		$event = $doc->getElementsByTagName('Event')->item(0)->nodeValue;
		$user = $doc->getElementsByTagName('FromUserName')->item(0)->nodeValue;
		$weixinId = $doc->getElementsByTagName('ToUserName')->item(0)->nodeValue;
		$time = time();

		$this->weixinlog($msg, $msgType, $event, $user, $weixinId);

		$text = '';
		$result = null;
		switch (strtolower($msgType)) {
			case "event":
				$event = strtolower($event);
				switch ($event) {
					case "click":
						$key = $doc->getElementsByTagName('EventKey')->item(0)->nodeValue;
						$result = $this->menuClick($key);
						break;
					case "subscribe":
					case "scan":
						$greeting = $this->greeting_msg($user);
						$key = $doc->getElementsByTagName('EventKey')->item(0)->nodeValue;
						if (empty($key)) {
							$result = $greeting;
						} else {
							if ($event == "subscribe") {
								if (!empty($greeting)) {
									$this->sendCustomMessage($this->weixinCustomMessageText($user, $greeting));
								}
								$key = substr($key, 8); /*strlen("qrscene_") = 8*/
							}
							$result = $this->getScanResult($user, $key);
						}
						break;
					default:
						return;
				}
				break;
			case "text":
				$content = $doc->getElementsByTagName('Content')->item(0)->nodeValue;
				$result = $this->textMessage($content);
				break;
			default:
				return;
		}

		if (empty($result))
			return;

		if (is_array($result)) {
			$text = $this->imagesReply($user, $weixinId, $time, $result);
		} else {
			$text = $this->textReply($user, $weixinId, $time, $result);
		}
		echo $this->encryptMsg($appConfig, $text, $timestamp, $nonce);
	}

	protected function getScanResult($user, $code) {
		return null;
	}

	protected function autoreplyrule() {
		return null;
	}

	protected function menuClick($key) {
		return null;
	}

	protected function greeting_msg($user) {
		return "欢迎";
	}

	protected function menuJson() {
		return "";
	}

	private function textMessage($content) {

		$rule = $this->autoreplyrule();
		if (empty($rule))
			return null;

		foreach ($rule as $rulegroup) {
			foreach ($rulegroup['keywords'] as $keyword) {
				if (strstr($content, $keyword) !== false) {
					return $rulegroup['reply'];
				}
			}
		} 
		
		return null;
	}

	protected function getJsSdkConfig($url) {
		$event = $this->currentEvent;

        $timeStamp = time();
		$nonce = uniqid('', true);
		$jsapi_ticket=$this->getJsApiTicket();
		if ($jsapi_ticket === false) {
			return false;
		}

		if (empty($url)) {
			$url = split('#', $_SERVER["HTTP_REFERER"])[0];
		} 

		$tmpArr = array("noncestr=$nonce", "timestamp=$timeStamp", "jsapi_ticket=$jsapi_ticket", "url=$url");
		$signature = $this->weixinSha1($tmpArr, '&');

		$appConfig = $this->getAppConfig($event);
		$appId = $appConfig[self::APPID];
		
		$data = array();
		$data['appId'] = $appId;
		$data['timestamp'] = $timeStamp;
		$data['nonceStr'] = $nonce;
		$data['signature'] = $signature;

		return $data;
	}

	protected function retrieveState() {
		return $_GET['state'];
	}

	protected function retrieveOpenId() {
		$code = $_GET["code"];
		if (empty($code)) {
			if (static::DEBUG) {
				echo "no code";
			}
			return false;
		}
        
        $json = $this->getAuthorizedInfo($code);
        if ($json === false) {
			if (static::DEBUG) {
				echo "auth failed";
			}
            return false;
		}

        $open_id = $json['openid'];
        if (empty($open_id)) {
			if (static::DEBUG) {
				echo "empty open id";
			}
            return false;
		}

        return $open_id;
    }
    
    private function authorizedUrl($redirectUrl, $scope, $state) {
        $appConfig = $this->getAppConfig($this->currentEvent);
		$appId = $appConfig[self::APPID];
        if (empty($appId))
            return false;

        $redirectUrl = \urlencode($redirectUrl);
        $url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=$appId&redirect_uri=$redirectUrl&response_type=code&scope=$scope" . (empty($state) ? "" : "&state=$state") . "#wechat_redirect";
        return $url;
    }

    protected function baseAuthorizeUrl($redirectUrl, $state = '') {
        return $this->authorizedUrl($redirectUrl, "snsapi_base", $state);
    }

    protected function userAuthorizeUrl($redirectUrl, $state = '') {
        return $this->authorizedUrl($redirectUrl, "snsapi_userinfo", $state);
    }

    protected function getAuthorizedInfo($code) {
		$appConfig = $this->getAppConfig($this->currentEvent);
		$appId = $appConfig[self::APPID];
		$appSecret = $appConfig[self::APPSECRET];
        if (empty($appId) || empty($appSecret)) {
			return false;
		}

        $json = $this->weixinApiInvoke("https://api.weixin.qq.com/sns/oauth2/access_token?appid=$appId&secret=$appSecret&code=$code&grant_type=authorization_code");
        if (empty($json)) {
			if (static::DEBUG) {
				var_dump($json);
			}
            return false;
		}
        return $json;
    }
	
	/*
	openid
	用户的唯一标识
	nickname
	用户昵称
	sex
	用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
	province
	用户个人资料填写的省份
	city
	普通用户个人资料填写的城市
	country
	国家，如中国为CN
	headimgurl
	用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空。若用户更换头像，原有头像URL将失效。
	privilege
	用户特权信息，json 数组，如微信沃卡用户为（chinaunicom）
	unionid
	只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。
	*/
	protected function getUserInfo($code) {
		if (empty($code))
			return false;

        $json = $this->getAuthorizedInfo($code);
        if ($json === false)
            return false;
        
        $access_token = $json['access_token'];
        $open_id = $json['openid'];

        if (empty($access_token) || empty($open_id))
            return false;
        $result = $this->weixinApiInvoke("https://api.weixin.qq.com/sns/userinfo?access_token=$access_token&openid=$open_id&lang=zh_CN");
        if (empty($result))
            return false;

        return $result;
	}

	protected function retrieveUserInfo() {
		$code = $_GET["code"];
		return $this->getUserInfo($code);
	}
	
	protected function getMiniProgramSession($mini_appid, $mini_appsecret, $code) {
		$appId = $mini_appid;
		$appSecret = $mini_appsecret;
		$result = $this->weixinApiInvoke("https://api.weixin.qq.com/sns/jscode2session?appid=$appId&secret=$appSecret&js_code=$code&grant_type=authorization_code");
		if (empty($result)) {
			return false;
		}
		return $result;
	}

	protected function getMiniProgramAccessToken($mini_appid, $mini_appsecret, $update = false, $force = false) {
		$appId = $mini_appid;
		$appSecret = $mini_appsecret;

        if (empty($appId) || empty($appSecret))
            return false;

		$key = "minip_access_token_". $appId;
		$access_token = $this->weixinToken($key, "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=$appId&secret=$appSecret", "access_token", false, $update, $force);
		return $access_token;
	}

	protected function weixinMiniProgramApiInvoke($apiUrl, $postJson, $timeout = Constant::HTTP_REQUEST_TIMEOUT) {
		if (empty($postJson))
			return null;

		$ch = curl_init();
		curl_setopt($ch, CURLOPT_TIMEOUT, $timeout);
		curl_setopt($ch,CURLOPT_URL, $apiUrl);
		curl_setopt($ch,CURLOPT_SSL_VERIFYPEER,FALSE);
		curl_setopt($ch, CURLOPT_HEADER, FALSE);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
	
		$headers[] = "Content-Type: application/json;charset=UTF-8";
		curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);

		curl_setopt($ch, CURLOPT_POST, TRUE);
		curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($postJson));
		
		$data['content'] = curl_exec($ch);
		$data['type'] = curl_getinfo($ch, CURLINFO_CONTENT_TYPE);
		curl_close($ch);

		$this->weixinlog( "$apiUrl " . var_export($postJson, true), "miniprogram_info", $data['type'], "", "");

		return $data;
	}

	protected function genMiniProgramQrCode($mini_appid, $mini_appsecret, $path)
	{
		$accessToken = $this->getMiniProgramAccessToken($mini_appid, $mini_appsecret);
		$opt = [
			'path' => $path,
		];

		$ret = $this->weixinMiniProgramApiInvoke("https://api.weixin.qq.com/wxa/getwxacode?access_token=$accessToken", $opt);
		ob_clean();
		header("Content-type: {$ret['type']}");
		echo $ret['content'];
		ob_end_flush();
		exit(0);
	}
}

trait WechatOperation
{
    public function index() 
    {
        $this->process();
    }

	// public function accesstoken() {
	// 	echo $this->getAccessToken();
	// }

	public function refreshtoken() {
		echo $this->getAccessToken(true, true);
	}

	public function refreshjssdkticket() {
		echo $this->getJsApiTicket(true, true);
	}

	public function installmenu() 
	{
		$json = $this->menuJson();
		if (empty($json))
			return;
		echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/menu/create?access_token=", true, $json, true);
	}

	// public function deletemenu() {
	// 	echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/menu/delete?access_token=", true, null, true);
	// }

	public function getselfmenu() {
		echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/get_current_selfmenu_info?access_token=", true, null, true);
	}

	public function getmenu() {
		echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/menu/get?access_token=", true, null, true);
	}
	
	private function getnewsmenu($item) {
		$newsmenu = array();
		foreach ($item['news_info']['list'] as $newsitem) {
			$newsmenu[] = array(
				'title' => $newsitem['title'],
				'digest' => $newsitem['digest'],
				'cover_url' => $newsitem['cover_url'],
				'content_url' => $newsitem['content_url'],
			);
		}
		return $newsmenu;
	}

	public function getmenuconfig() {
		$json = $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/get_current_selfmenu_info?access_token=", true);
		$menu_info = $json['selfmenu_info'];
		$newsmenu = array();
		foreach ($menu_info['button'] as &$item) {
			if (!empty($item['sub_button'])) {
				$item['sub_button'] = $item['sub_button']['list'];
			
				foreach ($item['sub_button'] as &$subitem) {
					if ($subitem['type'] == "news") {
						$newsmenu[] = $this->getnewsmenu($subitem);
						$subitem = array(
							'type' => 'click',
							'name' => $subitem['name'],
							'key' => "CLICK_" . count($newsmenu)
						);
					} else if ($subitem['type'] == "text") {
						$newsmenu[] = $subitem['value'];
						$subitem = array(
							'type' => 'click',
							'name' => $subitem['name'],
							'key' => "CLICK_" . count($newsmenu)
						);
					}
				}
			}
			if ($item['type'] == "news") {
				$newsmenu[] = $this->getnewsmenu($item);
				$item = array(
					'type' => 'click',
					'name' => $item['name'],
					'key' => "CLICK_" . count($newsmenu)
				);
			} else if ($item['type'] == "text") {
				$newsmenu[] = $item['value'];
				$item = array(
					'type' => 'click',
					'name' => $item['name'],
					'key' => "CLICK_" . count($newsmenu)
				);
			}
		}

		echo \json_encode($menu_info, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE);
		echo "\r\n\r\n";
		echo 'switch ($key) {' . "\r\n";
		for ($i = 0; $i < count($newsmenu); $i++) {
			echo "case \"CLICK_" . ($i+1) . "\" :\r\n";
			echo "return " . var_export($newsmenu[$i], true) . ";\r\n";
			echo "break;\r\n";
		}
		echo 'default:break;}';
	}
	
	public function getautoreply() {
		echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/get_current_autoreply_info?access_token=", true, null, true);
	}
	
	public function getautoreplyconfig() {
		$obj = $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/get_current_autoreply_info?access_token=", true);
		$rule = array();
		foreach ($obj['keyword_autoreply_info']['list'] as $item) {
			$rulegroup = array();
			$rulegroup['reply'] = $item['reply_list_info'][0]['content'];
			$rulegroup['keywords'] = array();
			foreach ($item["keyword_list_info"] as $keyword) {
				$rulegroup['keywords'][] = $keyword['content'];
			}
			$rule[] = $rulegroup;
		}

		var_export($rule);
	}

	public function getmaterials() {
		$obj = $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/material/get_materialcount?access_token=", true);
		$count = $obj['news_count'];

		$page = intval($count / 20);

		for ($i = 0; i <= $page; $i++) {
			$json = json_encode(array( "type" => "news", "offset" => $i * 20, "count" => 20));
			echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/material/batchget_material?access_token=", true, $json, true);
		}
	}

	// public function addmaterial() {
	// 	$file = curl_file_create("/mnt/m.d2c-china.cn/event/kose.jpg");
	// 	echo $this->weixinApiInvoke("https://api.weixin.qq.com/cgi-bin/material/add_material?type=image&access_token=", true, array("media" => $file));
    // }
}


?>