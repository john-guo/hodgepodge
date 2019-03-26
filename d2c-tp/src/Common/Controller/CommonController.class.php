<?php
namespace Common\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;
abstract class CommonController extends \Common\Controller\BaseController {
	protected function isLogin() {
		$id = $this->currentUid();
		if (!isset($id) || empty($id) || is_null($id)) {
			return false;
		}

		return true;
	}

	protected function checkLogin() {
		if (!$this->isLogin()) {
			$this->result(Constant::ERROR_USER_NOT_LOGIN);
		}
	}

	protected function userLogin($data) {
		$_SESSION['id'] = $data['id'];
		$_SESSION['account'] = $data['account'];
		$_SESSION['nickname'] = $data['nickname'];
	}

	protected function userSessionClear() {
		$_SESSION['id'] = null;
		$_SESSION['account'] = null;
		$_SESSION['nickname'] = null;
		unset($_SESSION['id']);
		unset($_SESSION['account']);
		unset($_SESSION['nickname']);
		session_unset();
	}

	protected function userLogout() {
		$this->userSessionClear();
		session_destroy();
	}

	protected function currentUid() {
		return $_SESSION['id'];
	}

	protected function currentAccount() {
		return $_SESSION['account'];
	}

	protected function currentNickname() {
		return $_SESSION['nickname'];
	}

	protected function addlog($op, $target = '', $name = '', $memo = '')
	{
		$log = $this->getModel("logs");

		$data = array();

		if ($this->isLogin())
	        $data['operator_id'] = $this->currentUid();
		else 
			$data['operator_id'] = 0;
		$data['op'] = $op;
        $data['name'] = $name;
        $data['target'] = $target;
        $data['memo'] = $memo;
		$data['ip'] = Utils::getIpAddress();
        $log->add($data);
	}

	protected function genQrImg($url, $fgcolor = null) {
		vendor("vendor.autoload");
		$qrCode = new \Endroid\QrCode\QrCode($url);
		//$qrCode->setSize(300);

		// Set advanced options
		//$qrCode->setWriterByName('png');
		//$qrCode->setMargin(10);
		//$qrCode->setEncoding('UTF-8');
		//$qrCode->setErrorCorrectionLevel(ErrorCorrectionLevel::HIGH);
		if (!empty($fgcolor))
			$qrCode->setForegroundColor($fgcolor);
		//$qrCode->setBackgroundColor(['r' => 255, 'g' => 255, 'b' => 255, 'a' => 0]);
		// $qrCode->setLabel('Scan the code', 16, __DIR__.'/../assets/fonts/noto_sans.otf', LabelAlignment::CENTER);
		// $qrCode->setLogoPath(__DIR__.'/../assets/images/symfony.png');
		// $qrCode->setLogoWidth(150);
		//$qrCode->setRoundBlockSize(true);
		//$qrCode->setValidateResult(false);
		
		// Directly output the QR code
		// header('Content-Type: '.$type);
		// echo $qrCode->writeString();

		$type = $qrCode->getContentType();
		return "data:$type;base64," . base64_encode($qrCode->writeString());
	}

	protected function captcha() {
		vendor("vendor.autoload");

		$phrases = new \Gregwar\Captcha\PhraseBuilder(4, '0123456789');
		$builder = new \Gregwar\Captcha\CaptchaBuilder(null, $phrases);
		$builder->build();
		$_SESSION['phrase'] = $builder->getPhrase();
		header('Content-type: image/jpeg');
		$builder->output();
	} 
}