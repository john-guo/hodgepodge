<?php
namespace Common\Controller;
use \Common\Common\Constant;
abstract class ApiController extends \Think\Controller {
    function _initialize() {
		//header("Access-Control-Allow-Origin: *");
	}

    protected function _empty() {
    }

    protected function jump($url) {
        header("location: $url");
        exit;
    }

    protected function makeSiteUrl($path) {
        return Constant::HOST . $path;
    }

    protected function requestParameter($name) {
        return I($name);
    }

    protected function getNoKeyParameter() {
        $parameters = $this->requestParameter("get.");
        if (count($parameters) == 0)
            return '';
        return array_keys($parameters)[0];
    }

    protected function getModel($tableName) {
        return M($tableName);
    }

    protected function getEmptyModel() {
        return M();
    }

    protected function getBindParameters(array $array)
    {
        $ret = array();
        foreach ($array as $key => $value) 
        {
            $ret[$key] = ":$key";
        }

        return $ret;
    }

    protected function getBind(array $array)
    {
        $ret = array();
        foreach ($array as $key => $value)
        {
            $ret[":$key"] = $value;
        }

        return $ret;
    }
}