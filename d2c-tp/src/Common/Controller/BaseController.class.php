<?php
namespace Common\Controller;
use \Common\Common\Utils;
use \Common\Common\Constant;
use \Common\Controller\ApiController;
abstract class BaseController extends ApiController {
	protected function errorMsg($error) {
		return Constant::$ERROR_MSGS[$error];
	}

	protected function resultMessage($error, $msg, $data = null) {
		$ret = array();
		$ret['result'] = $error;
		$ret['msg'] = $msg;
		$ret['data'] = $data;
		$this->ajaxReturn($ret);
	}

	protected function result($error, $data = null) {
		$this->resultMessage($error, $this->errorMsg($error), $data);
	}

	protected function OK($data = null)
	{
		$this->result(Constant::ERROR_OK, $data);
	}

	protected function checkDb($ret, $func = null) {
		if ($ret === false) {
			if (!is_null($func)) {
				call_user_func($func);
			}
			$this->result(Constant::ERROR_SYS_DB);
		}
	}

	protected function getPageParameters() {
		$page = I("page/d");
        $rows = I("rows/d");
        if (!is_numeric($page) || $page <= 0)
            $page = 1;
        if (!is_numeric($rows) || $rows <= 0)
			$rows = 10;
			
		return array("page"=>$page, "rows"=>$rows);
	}

	protected function uploadFiles() {
		$upload = new \Think\Upload();
		$upload->rootPath = Constant::getRoot() . Constant::UPLOAD . '/';
		$upload->subName = date('Y') . '/' . date('m') .'/' . date('d');
		
		$res = $upload->upload();
		$data = array();
		if ($res === false) {
			$data['result'] = Constant::ERROR_FAILED;
			$data['msg'] = $upload->getError();
		} else {
			$data['result'] = Constant::ERROR_OK;
			$data['urls'] = array();
			foreach ($res as $key => $item) {
				$url = '/' .Constant::UPLOAD . '/' . $item['savepath'] . $item['savename'];
				$data['urls'][] = array(
					'name' => $key,
					'url' => $this->makeSiteUrl($url)
				);
			}
		}

		return $data;
	}

	protected function uploadFilesBase64($filename, $base64content) {
		$rpath = Constant::UPLOAD . '/' . date('Y') . '/' . date('m') .'/' . date('d') . '/';
		$path = Constant::getRoot() . $rpath;
		if (!is_dir($path)) {
			mkdir($path,0777,true);
		}
		$savefile = $path . $filename;
		file_put_contents($savefile, base64_decode($base64content));
		return $this->makeSiteUrl('/' . $rpath . $filename);
	}

	protected function map_store($key, $value, $timeout = 0)
	{
		$redis = new \Redis();
		$redis->pconnect("localhost");
		if (empty($timeout))
		{
			$redis->set($key, $value);
		}
		else 
		{
			$redis->setEx($key, $timeout, $value);
		}
		$redis->close();
	}

	protected function map_retrieve($key) 
	{
		$redis = new \Redis();
		$redis->pconnect("localhost");
		$value = $redis->get($key);
		$redis->close();
		return $value;
	}

	protected function retrieveCode($id) {
		$code = Utils::genCode();
		$this->map_store($id, $code, Constant::CODE_TIMEOUT);
		return $code;
	}

	protected function checkCode($id, $code) {
		$scode = $this->map_retrieve($id);
		if (!empty($code) && $scode !== FALSE && $scode === $code)
		{
			return true;
		}
		return false;
	}
}