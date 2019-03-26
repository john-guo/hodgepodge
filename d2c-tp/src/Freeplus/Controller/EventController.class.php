<?php
namespace Freeplus\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;
class EventController extends \Common\Controller\WechatController {
    use \Common\Controller\WechatOperation;

    const SALT = "plusfree";
    const PASSWORD = "fp2018";

    const POPUP_TITLE = "freeplus线下活动";
    const POPUP_CONTENT = "12.7-12.12西安赛格1F 双重惊喜等你来！";
    const POPUP_PIC = "http://n.d2c-china.cn/event/freeplus/img/title.png";

    protected function getAppConfig($event) {
        return array(
			self::APPID => "wxea2562dce2fe2e54",
			self::APPSECRET => "f45df94b3c6f7ef28e06e1da1ae1c1bd",
			self::AESKEY => "nemROLLdw84No7XQ2bcvbBVgw0hytbzLgUw16EoKBjB",
			self::TOKEN => "freeplus",
    	);
    }

    private function loginAdmin() {
        $_SESSION['admin'] = 'admin';
    }

    private function isAdmin() {
        $admin = $_SESSION['admin'];
        if (empty($admin) || $admin != 'admin') {
            return false;
        }
        return true;
    }

    private function checkAdmin() {
        if (!$this->isAdmin()) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
    }
    
    public function register() {
        $mobile = $this->requestParameter('mobile');
        if (empty($mobile)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $userModel = $this->getModel("users");
        $ret = $userModel->where(array('mobile' => $mobile))->find();
        $this->checkDb($ret);

        $id = null;
        if (empty($ret)) {
            $id = $userModel->add(array('mobile' => $mobile, 'create_ts' => Utils::now()));
            $this->checkDb($id);
        } else {
            $id = $ret['id'];
            if (!empty($ret['status'])) {
                $ret = $userModel->where(array('id' => $id))->setField(["status" => 0]);
                $this->checkDb($ret);
            }
        }

        if (empty($id)) {
            $this->result(Constant::ERROR_FAILED);
        }

        $data = array(
            "url" => $this->genQrUrl($id, null)
        );

        $this->addlog("register", $mobile, $uid);
        $this->OK($data);
    }

    public function info() {
        $code = $this->requestParameter("code");

        $parameter = Utils::get_parameter($code, self::SALT);
        if (empty($parameter)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $uid = $parameter['uid'];
        $userModel = $this->getModel("users");
        $user = $userModel->where(array('id' => $uid))->find();
        $this->checkDb($user);
        if (empty($user)) {
            $this->result(Constant::ERROR_FAILED);
        }

        $code = Utils::gen_parameter('1', $user['id'], self::SALT);
        $url1 = "http://n.d2c-china.cn/event/freeplus/check.html?code=$code";

        $code = Utils::gen_parameter('2', $user['id'], self::SALT);
        $url2 = "http://n.d2c-china.cn/event/freeplus/check.html?code=$code";

        $data = array(
            "gift_1" => array(
                "qr" => $this->genQrImg($url1),
                "status" => !empty($user['gift_1'])
            ),
            "gift_2" => array(
                "qr" => $this->genQrImg($url2),
                "status" => !empty($user['gift_2'])
            )
        );

        $this->addlog("info", $uid);
        $this->OK($data);
    }

    public function check() {
        $code = $this->requestParameter("code");

        $parameter = Utils::get_parameter($code, self::SALT);
        if (empty($parameter)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }

        $uid = $parameter['uid'];
        $userModel = $this->getModel("users");
        $user = $userModel->where(array('id' => $uid))->find();
        $this->checkDb($user);
        if (empty($user)) {
            $this->result(Constant::ERROR_FAILED);
        }

        $data = array(
            "gift_1" => !empty($user['gift_1']),
            "gift_2" => !empty($user['gift_2']),
            "status" => $user['status']
        );

        $this->OK($data);
    }

    public function admin() {
        $password = $this->requestParameter("password");
        if ($password != self::PASSWORD) {
            $this->result(Constant::ERROR_PERM_DENY);
        }
        $this->loginAdmin();
        $this->addlog("admin");
        $this->OK();
    }

    public function gift() {
        $this->checkAdmin();
        
        $code = $this->requestParameter("code");

        $parameter = Utils::get_parameter($code, self::SALT);
        if (empty($parameter)) {
            $this->result(Constant::ERROR_INVALID_PARAMETERS);
        }
        $id = $parameter['id'];
        $uid = $parameter['uid'];

        $userModel = $this->getModel("users");
        $user = $userModel->where(array('id' => $uid))->find();
        $this->checkDb($user);
        if (empty($user)) {
            $this->result(Constant::ERROR_FAILED);
        }
        
        if ($user["gift_$id"] != 0) {
            $this->OK();
        }

        $ret = $userModel->where(array('id' => $uid, "gift_$id" => 0))->setField(array("gift_$id" => 1));
        $this->checkDb($ret);

        $this->addlog("scan", $uid, "$id");
        $this->OK();
    }

    protected function getScanResult($code) {
        if (empty($code))
            return null;

        $userModel = $this->getModel("users");
        $user = $userModel->where(array('id' => $code))->find();
        if (empty($user)) {
            $this->addlog("scan_result_error", "$code");
            return null;
        }

        if (empty($user['status'])) {
            $userModel->where(array('id' => $code))->setField(["status" => 1]);
        }

        $code = Utils::gen_parameter('0', $user['id'], self::SALT);
        $this->addlog("scan_result", $user['id'], $code);
        return 
        array (
            array(
                'title' =>  self::POPUP_TITLE,
                'digest' => self::POPUP_CONTENT,
                'cover_url' => self::POPUP_PIC,
                'content_url' => "http://n.d2c-china.cn/event/freeplus/index.html?code=$code",
            )
        );
    }

    protected function menuJson() {
		$json = <<<EOT
		{
			"button": [{
					"name": "谢谢敏感",
					"sub_button": [{
							"type": "click",
							"name": "田馥甄的codebox",
							"key": "CLICK_1"
					}, {
							"type": "click",
							"name": "HEBE手机壁纸下载",
							"key": "CLICK_2"
					}, {
							"type": "view",
							"name": "芙品大全",
							"url": "http://www.freeplus.cn/freeplus/lineup/"
					}]
			}, {
					"type": "view",
					"name": "最新活动",
					"url": "http://mp.weixin.qq.com/s?__biz=MjM5NzkxMzU1Mg==&mid=2649824706&idx=1&sn=82dab18883b528ef2143015fcbab44ef&chksm=bed70b7d89a0826bc14ec53796e560548e336676d7118511d2ff3ffacc3817e278ead14172a3&scene=18#wechat_redirect"
			}, {
					"name": "寻找小芙",
					"sub_button": [{
							"type": "click",
							"name": "官方天猫",
							"key": "CLICK_3"
					}, {
							"type": "view",
							"name": "全国店铺",
							"url": "http://www.freeplus.cn/freeplus/shops/"
					}]
			}]
	}
EOT;
		return $json;
	}

    protected function menuClick($key) {
        switch ($key) {
            case "CLICK_1" :
            return array (
                0 => 
                array (
                    'title' => 'freeplus_2018谢谢敏感丨打开田馥甄的敏感Codebox',
                    'digest' => '当我们谈论敏感的时候，在谈什么？',
                    'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/8DYxCw3INX5pLlW7YmT2UcicoYtlF7e5UWLufyygGtvzJ8kfkIEjM5sek7Xav4SNU3Ohr3NfcIN57kib0lzWVGUg/0?wx_fmt=jpeg',
                    'content_url' => 'http://mp.weixin.qq.com/s?__biz=MjM5NzkxMzU1Mg==&mid=502340547&idx=1&sn=55bb9dcccfbec7aa384c0584e55d0877&chksm=3ed70d7c09a0846af229c95a51a2b1ea38950674d1a3d0bf089cdf1d14e932e8eb47de563496#rd',
                ),
            );
            break;
            case "CLICK_2" :
            return array (
                0 => 
                array (
                    'title' => '壁纸│田馥甄HEBE手机壁纸下载',
                    'digest' => '2018SS 最新版',
                    'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/8DYxCw3INX5kn78Bng3kPzOe93SJRVgiaklOUQ5CkEBnIRX1buJGtQKA09G4NP3hrzjcfOnwuURCU0icPdHibDN5Q/0?wx_fmt=jpeg',
                    'content_url' => 'http://mp.weixin.qq.com/s?__biz=MjM5NzkxMzU1Mg==&mid=502339869&idx=1&sn=2d1609b0da1a55861d78e468cebd0146&chksm=3ed70fa209a086b4edd1d3800d0cbb47ab5f0877224928a1d5d21c9a0a62d11db225fb0c2551#rd',
                ),
            );
            break;
            case "CLICK_3" :
            return '欢迎光临小芙家的天猫旗舰店
官方正品！赶紧买买买起来~

❀❀❀

【freeplus芙丽芳丝官方旗舰店】https://link.laiwang.com/agent/mobile.htm?agentId=110213&_bind=true点击链接，再选择浏览器打开；或复制￥l1BNbPOKwKe￥这条信息后打开淘宝

❀❀❀';
            break;
            default:break;}

		return null;
	}

	protected function greeting_msg($user) {
		return "感谢您关注小芙！
谢谢敏感，与freeplus一起。给敏感的心和不安定的肌肤减负。
 
💧点击菜单【谢谢敏感】即可了解田馥甄2018年首支主题CM，获取HEBE手机壁纸和芙品大全~
 
💧点击菜单【最新活动】即可获取更多活动福利哟~
 
💧点击菜单【寻找小芙】即可获取全国店铺信息和天猫旗舰店~";
	}

	protected function autoreplyrule() {
		return array (
            0 => 
            array (
              'reply' => 'k7wGok2HsU7dGjXS66WLXXey_dbeIrUKbiltc33luaE',
              'keywords' => 
              array (
                0 => '深吻水开奖',
              ),
            ),
            1 => 
            array (
              'reply' => '测试中',
              'keywords' => 
              array (
                0 => '派样',
              ),
            ),
            2 => 
            array (
              'reply' => 'k7wGok2HsU7dGjXS66WLXX-tZ8ozI_Jn9FbHXw94EhE',
              'keywords' => 
              array (
                0 => '哆啦开奖',
              ),
            ),
            3 => 
            array (
              'reply' => 'k7wGok2HsU7dGjXS66WLXX-tZ8ozI_Jn9FbHXw94EhE',
              'keywords' => 
              array (
                0 => '十年感悟',
              ),
            ),
            4 => 
            array (
              'reply' => 'k7wGok2HsU7dGjXS66WLXXnDIPtx2VTgyJEICYtslGQ',
              'keywords' => 
              array (
                0 => '自己的房间开奖',
              ),
            ),
            5 => 
            array (
              'reply' => 'k7wGok2HsU7dGjXS66WLXXi70IA6bWoy7WzVbR0PVfY',
              'keywords' => 
              array (
                0 => '双重清洁开奖',
              ),
            ),
            6 => 
            array (
              'reply' => '感谢您的支持！
点击：app.rteam.cn/180605freeplus获取领赠码，即刻领取freeplus芙丽芳丝明星产品--净润洗面霜中样1支吧！',
              'keywords' => 
              array (
                0 => '机场',
                1 => '機場',
              ),
            ),
            7 => 
            array (
              'reply' => '您好，芙丽芳丝工厂位于神奈川县，并非辐射区。产品的上标注的东京都地址是日本佳丽宝总公司地址，并非工厂地址。感谢您对小芙的关注。/可爱',
              'keywords' => 
              array (
                0 => '辐射',
                1 => '生产地',
                2 => '产地',
                3 => '东京都',
              ),
            ),
            8 => 
            array (
              'reply' => '感谢您对芙丽芳丝品牌的关注和厚爱~芙丽芳丝产品一直以来秉持着“近零刺激”的理念。如您正值怀孕或哺乳期间，考虑到每个人体质不同，为了确保万无一失，建议您最好前往专柜先做局部测试哦~
          
全国各大专柜店铺查询↓↓
          http://www.freeplus.cn/freeplus/shops/',
              'keywords' => 
              array (
                0 => '怀孕',
                1 => '孕妇',
                2 => '孕期',
                3 => '哺乳',
              ),
            ),
            9 => 
            array (
              'reply' => '[亲亲]感谢您一直以来对芙丽芳丝品牌的厚爱~

目前获得官方授权的经销商有：
芙丽芳丝官方天猫店、仪菲、京东芙丽芳丝自营官方店、唯品会。

针对芙家产品真假和购买渠道的问题，小芙一直以来强调购产品一定要在官方授权的正规渠道购买哟！

正品购买请认准我们的官方天猫旗舰店↓↓
❀❀❀
【freeplus芙丽芳丝官方旗舰店】https://freeplus.tmall.com/点击链接，再选择浏览器打开
❀❀❀

全国各大专柜店铺查询↓↓
http://www.freeplus.cn/freeplus/shops/',
              'keywords' => 
              array (
                0 => '真假',
                1 => '正品',
                2 => '唯品会',
                3 => '京东',
                4 => '聚美',
                5 => '是真的吗',
                6 => '假',
                7 => '授权',
                8 => '仪菲',
              ),
            ),
        );
    }
    
}