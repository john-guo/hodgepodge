<?php
namespace Events\Common;

use \Common\Common\Constant;
use \Common\Common\Utils;
use \Common\Controller\CommonController;

abstract class AdminBaseController extends CommonController {

    protected function userLogin($data) {
		$_SESSION['admin'] = $data['id'];
		$_SESSION['admin_level'] = $data['level'];
		$_SESSION['admin_account'] = $data['account'];
	}

	protected function userSessionClear() {
		$_SESSION['admin'] = null;
		$_SESSION['admin_level'] = null;
		$_SESSION['admin_account'] = null;
		unset($_SESSION['admin']);
		unset($_SESSION['admin_level']);
		unset($_SESSION['admin_account']);
		session_unset();
    }
    
	protected function currentUid() {
		return $_SESSION['admin'];
	}

	protected function currentAccount() {
		return $_SESSION['admin_account'];
	}

	protected function currentNickname() {
		return $_SESSION['admin_account'];
	}

    protected function isSupervisor() {
		return $_SESSION['admin_level'] === '0';
	}
}