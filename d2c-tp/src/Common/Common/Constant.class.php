<?php
namespace Common\Common;
class Constant {

    static function getRoot() {
        return APP_PATH . "../";
    }

	const HOST = "https://n.d2c-china.cn";
    const UPLOAD = 'uploads';
    const AVATAR_UPLOAD = 'static/head';

    const HTTP_REQUEST_TIMEOUT = 30;
    const CODE_TIMEOUT = 60;
    const DEFAULT_WECHAT_PUBLIC_TICKET = 'PUBLIC';

    const SMS_OPTIONS_TOKEN = "SMS_TOKEN";
    const SMS_OPTIONS_SOAP_SERVICE = "SMS_SERVICE";
    const SMS_OPTIONS_SOAP_USER = "SMS_USER";
    const SMS_OPTIONS_SOAP_PASSWORD = "SMS_PASSWORD";
    const SMS_OPTIONS_SOAP_CORP = "SMS_CORP";
    const SMS_OPTIONS_SOAP_PRODUCT = "SMS_PRODUCT";

    const ERROR_NOT_SUPPORT = -7;
    const ERROR_RECORD_NOTFOUND = -6;
    const ERROR_TIMEOUT = -5;
    const ERROR_PERM_DENY = -4;
    const ERROR_INVALID_PARAMETERS = -3;
    const ERROR_SYS_DB = -2;
    const ERROR_FAILED = -1;
    const ERROR_OK = 0;
    const ERROR_USER_NOT_REGISTER = 1000;
    const ERROR_USER_NOT_LOGIN = 1001;
    const ERROR_USER_FULLEGG = 1002;
    const ERROR_USER_SMS = 1003;
    const ERROR_USER_ALREADY_SETTLE = 1004;
    const ERROR_USER_ITEMS_NOT_MATCH = 1005;
    const ERROR_USER_AUTHORING = 1006;
    const ERROR_USER_ITEM_DONE = 1007;
    const ERROR_USER_ITEM_STATUS_WRONG = 1008;
    const ERROR_USER_ALREADY_ENJOY = 1009;
    const ERROR_USER_NOT_IN_QUEUE = 1010;
    const ERROR_USER_ALREADY_REGISTER = 1011;
    const ERROR_ITEM_NOT_COMPLETE = 1012;
    const ERROR_USER_CITY_NOTFOUND = 1013;
    const ERROR_USER_DATE_NOTMATCH = 1014;
    const ERROR_USER_FULL_REWARD = 1015;
 
    static $ERROR_MSGS = array(
        self::ERROR_NOT_SUPPORT => "接口不支持",
        self::ERROR_RECORD_NOTFOUND => "数据未找到",
        self::ERROR_TIMEOUT => "操作超时",
        self::ERROR_PERM_DENY => "操作禁止",
        self::ERROR_INVALID_PARAMETERS => "参数非法",
        self::ERROR_SYS_DB => "系统错误",
        self::ERROR_FAILED => "失败", 
        self::ERROR_OK => "成功", 
        self::ERROR_USER_NOT_REGISTER => "用户未留资",
        self::ERROR_USER_NOT_LOGIN => "用户未登陆",
        self::ERROR_USER_FULLEGG => "已没有彩蛋",
        self::ERROR_USER_SMS => "验证码不匹配",
        self::ERROR_USER_ALREADY_SETTLE => "用户已结算",
        self::ERROR_USER_ITEMS_NOT_MATCH => "条件未满足",
        self::ERROR_USER_AUTHORING => "审核中",
        self::ERROR_USER_ITEM_DONE => "已完成",
        self::ERROR_USER_ITEM_STATUS_WRONG => "项目状态错",
        self::ERROR_USER_ALREADY_ENJOY => "无法重复参加",
        self::ERROR_USER_NOT_IN_QUEUE => "不在队列中",
        self::ERROR_USER_ALREADY_REGISTER => "用户已注册",
        self::ERROR_ITEM_NOT_COMPLETE => "项目未完成",
        self::ERROR_USER_CITY_NOTFOUND => "所选城市未找到",
        self::ERROR_USER_DATE_NOTMATCH => "日期不匹配",
        self::ERROR_USER_FULL_REWARD => "已无奖品",
    );

    const FLAG_NO = 0;
    const FLAG_YES = 1;

    const STATUS_INIT = 0;
    const STATUS_DONE = 1;
    const STATUS_CONFIRMED = 2;

    const TYPE_NORMAL = 0;
    const TYPE_AUTH = 1;
}
