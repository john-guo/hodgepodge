<?php
namespace Common\Common;
class Utils {
    public static function now() {
		return date('Y-m-d H:i:s');
	}

	public static function today() {
		return date('Y-m-d');
	}

	public static function getDate($datetime) {
		return date('Y-m-d', strtotime($datetime));
	}

	public static function getExcelTime($datetime) {
		return date("Y/m/d H:i:s", strtotime($datetime));
	}

	public static function getMoneyNumber($num) {
		return number_format($num / 100, 2, '.', ''); 
	}

	public static function genCode() {
		$randStr = str_shuffle("0123456789");
		return substr($randStr, 0, 6);
	}

	public static function genKey() {
		return self::makedigest(rand(), rand(), rand()) . self::makedigest(rand() + 1, rand() + 2, rand() + 3);
	}

	public static function sendSms($mobile, $code, $options, $useSoap = false) {

		//const SMS_TOKEN = "7100239230898406";

		$template = "您的验证码是 $code 。有效时间为5分钟，请尽快验证。";

		if (!$useSoap) {
			$content = urlencode($template);
			$date = urlencode("2010-1-1");
			$token = $options[Constant::SMS_OPTIONS_TOKEN];
			$ret = Utils::curl("http://www.wemediacn.net/webservice/smsservice.asmx/SendSMS?mobile=$mobile&FormatID=8&Content=$content&ScheduleDate=$date&TokenID=$token", false);
			$doc = new \DOMDocument();
			$doc->loadXML($ret);
			$result = $doc->getElementsByTagName('string')->item(0)->nodeValue;
	
			if (substr($result, 0, 2) == "OK")
				return true;

			return $result;
		}

		libxml_disable_entity_loader(false);
		$soap = new \SoapClient($options[Constant::SMS_OPTIONS_SOAP_SERVICE], array('exceptions' => 0));
		$params = array(
			"sname" => $options[Constant::SMS_OPTIONS_SOAP_USER], //'dldex01',
			"spwd" => $options[Constant::SMS_OPTIONS_SOAP_PASSWORD], //'d2cChina',
			"scorpid" => $options[Constant::SMS_OPTIONS_SOAP_CORP], //'kagome',
			"sprdid" => $options[Constant::SMS_OPTIONS_SOAP_PRODUCT],
			"sdst" => $mobile,
			"smsg" => $template
		);

		$ret = $soap->__soapCall("g_Submit", array('parameters'=>$params));
		if (is_soap_fault($ret)) {
			return false;
		}

		$result = $ret->g_SubmitResult;
		if ($result->State == '0')
			return true;

		return json_encode($result);
	}

  private static function curl($url, $data, $json, $headers, $timeout, $clierror, $debug) {
		$ch = curl_init();
		curl_setopt($ch, CURLOPT_TIMEOUT, $timeout);
		curl_setopt($ch,CURLOPT_URL, $url);
		curl_setopt($ch,CURLOPT_SSL_VERIFYPEER,FALSE);
		//curl_setopt($ch,CURLOPT_SSL_VERIFYHOST,2);//严格校验
		curl_setopt($ch, CURLOPT_HEADER, FALSE);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
	
		if ($json === true) {
			if (!is_array($headers))
			{
				$headers = array();
			}

			$headers[] = "Content-Type: application/json;charset=UTF-8";
		}

		if (!empty($headers))
			curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);

		if (!empty($data)) {
			curl_setopt($ch, CURLOPT_POST, TRUE);
			curl_setopt($ch, CURLOPT_POSTFIELDS, $data);
		}
		
		if ($debug) {
			curl_setopt($ch, CURLINFO_HEADER_OUT, TRUE);
		}

		//运行curl
		$ret = curl_exec($ch);
		
		if (!$ret && $clierror) {
			echo Utils::now() . " Network error:" . curl_error($ch) . PHP_EOL;
		}
		
		if ($debug) {
			echo curl_getinfo($ch, CURLINFO_HEADER_OUT);
			echo $data;	
		}

		curl_close($ch);

		return $ret;
    }

	private static function curlJson($url, $data, $headers, $timeout, $clierror, $debug) {
		return Utils::curl($url, $data, true, $headers, $timeout, $clierror, $debug);
	}

	public static function curlJsonGet($url, $headers = null, $timeout = Constant::HTTP_REQUEST_TIMEOUT, $clierror = false, $debug = false) {
		return Utils::curlJson($url, null, $headers, $timeout, $clierror, $debug);
	}

	public static function curlJsonPost($url, $data, $headers = null, $timeout = Constant::HTTP_REQUEST_TIMEOUT, $clierror = false, $debug = false) {
		return Utils::curlJson($url, $data, $headers, $timeout, $clierror, $debug);
	}

	public static function my_floor($number, $precision = 2) {
		$fig = (int) str_pad('1', $precision + 1, '0');
		return (floor($number * $fig) / $fig);
	}

	public static function pickJPYPrice($cny, $rate) {
		$jpy2cny = $rate;
		$cny2jpy = 1.0 / $jpy2cny;
		return floor($cny * $cny2jpy);
	}

	public static function pickCNYPrice($jpy, $rate) {
		$jpy2cny = $rate;
		$cny2jpy = 1.0 / $jpy2cny;
		return Utils::my_floor($jpy * $jpy2cny, 2);
	}

	private static function makedigest($id, $uid, $salt) {
		$digest = md5($salt . "_" . chr($id) . "_" . $salt . "_" . $uid . "_" . $salt);
		return substr($digest, 1, 8);
	}

	public static function gen_parameter($id, $uid, $salt = '') {
		$digest = self::makedigest($id, $uid, $salt);
		$id = bin2hex(pack("N", $id));
		$parameter = $id . "" . $digest . "" . $uid;
		return $parameter;
	}

	public static function get_parameter($code, $salt = '') {
		if (strlen($code) < 17)
			return null;
		$data = array();
		$data['id'] = $id = unpack("N", hex2bin(substr($code, 0, 8)))[1];
		$data['uid'] = $uid = substr($code, 16);
		$digest = self::makedigest($id, $uid, $salt);
		if ($digest !== substr($code, 8, 8))
				return null;

		return $data;
	}
	
	public static function getIpAddress() {
		return get_client_ip();
	}
}