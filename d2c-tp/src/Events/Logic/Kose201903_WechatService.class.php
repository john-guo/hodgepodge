<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IWechat;

class Kose201903_WechatService extends \Events\Common\WechatService implements IWechat
{
    const POPUP_TITLE = '';
    const POPUP_CONTENT = '';
    const POPUP_PIC = '';
    const POPUP_URL = 'http://n.d2c-china.cn/event/run2019/index.html';

    public function onUserScan($user, $code) {
      if (empty($code))
          return null;

      $this->weixinlog('', 'scan_result', $code, '', $user);

      switch ($code) {
        case "PUBLIC":
          return 
          array(
              array(
                'title' =>  '黛珂樱花梦享空间邀请函',
                'digest' => '黛珂约你一起，因己而悦尽兴而美',
                'cover_url' => 'https://n.d2c-china.cn/event/alladmin/img/wel_1.png',
                'content_url' => 'https://n.d2c-china.cn/event/alladmin/kose_wel.html',
              ),
          );
        break;

        case "SAKURA":
        
          return 
          array(
              array(
                'title' =>  '邀您步入黛珂樱花梦享空间',
                'digest' => '早春三月，黛珂邀请一起来赏樱',
                'cover_url' => 'https://n.d2c-china.cn/images/sakura.jpg',
                'content_url' => 'https://h5.saintse.cn/daike/syh.php',
              ),
          );

        break;

        case "EXP":
      
          return 
          array(
              array(
                'title' =>  '赶紧点击领取神秘好礼~',
                'digest' => '活动3月18日至3月24日，送完即止~',
                'cover_url' => 'https://n.d2c-china.cn/images/exp.jpg',
                'content_url' => 'https://h5.saintse.cn/daike/tyz.php',
              ),
          );

        break;

        default:
        break;
      }

        // $sakura = array(
        //   'title' =>  '欢迎踏入黛珂绮丽樱花梦境',
        //   'digest' => '早春三月，黛珂邀请一起来赏樱',
        //   'cover_url' => 'https://n.d2c-china.cn/images/sakura.jpg',
        //   'content_url' => 'https://h5.saintse.cn/daike/syh.php',
        // );

        // $msg = $this->weixinCustomMessageNews($user, $sakura['title'], $sakura['digest'], $sakura['cover_url'], $sakura['content_url']);

        // $this->sendCustomMessage($msg);

      return null;
      // $url = self::POPUP_PIC . '?url=' . urlencode($content);
      // $this->addlog("scan_result", $code, $url);
      // return 
      // array (
      //     array(
      //         'title' =>  self::POPUP_TITLE,
      //         'digest' => self::POPUP_CONTENT,
      //         'cover_url' => self::POPUP_PIC,
      //         'content_url' => $url,
      //     )
      // );
    }
    
    public function onMenuClick($key) {
      switch ($key) {
        case "CLICK_1":
          return array(
            0 => array(
              'title' => '黛珂珍颜会 | 至臻礼遇，许你美的勋章。', 
              'digest' => '感谢您成为黛珂「珍颜会」会员。期待黛珂能与您一同成就更具光彩的美丽。', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG1ibOna5Q5Rpu9h8jRhptRhLibZ9ibTAPibVJ2CamkfZOGwJVY9vxNd4xTmHeUtFBHs0ufiaXVHoayVUTibg/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502439826&idx=1&sn=f20e941403e948963cc06de84058f81f&chksm=0831e8313f4661273714d8dbfebc59aa3c437616655f18361467fbc7e61205303e4d815cd2fb#rd', 
            ), 
          );
          break;
        case "CLICK_2":
          return array(
            0 => array(
              'title' => '黛珂——美的勋章，献给臻贵的你', 
              'digest' => '进化无止境，黛珂，不会止步于现在。', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG1ibuzlicSX9SsOEfcfG9rcge7z5VjC8NLgvvryMAg5JZJ4opP7P86zhRPKibusnRw7iacuibtLkkh34t0g/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438717&idx=1&sn=215ef6a6bf625671f630a1e8e3a1cf31&chksm=0831ec9e3f46658834a6781884fbc5cacfe2fbf1bc3a4b500b9d64483bec3c50fd5fb6ba9228#rd', 
            ), 
          );
          break;
        case "CLICK_3":
          return array(
            0 => array(
              'title' => '保湿美容液系列', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG1icVWK48xFh4mZhhNspaVLMwiao7Ijx2JSrwoTlr6micfib7QFicib7WLurldJ7MLgff4JjUx5MZbGyzOuA/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=1&sn=bfc6ac5108b3e015906f001182b78460&chksm=0831ec3a3f46652c0fe6dd04d9b010cd7be306cc3c90a602719c8cc49c65b4fae9d5f386af51#rd', 
            ), 
            1 => array(
              'title' => 'AQ珍萃精颜系列', 
              'digest' => 'AQ珍萃精颜 悦活洁肤霜 150g￥800以醇厚的质感包裹肌肤，深入洁净彩妆和皮脂，让肌肤悦现清透光泽的洁肤', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19Fapuom2rMEQpbM1s3ZmFHxSKIib8NRHbcN1z5YlO3ovBCgian6e5AxNlssOCxtEGfdc6x8Vr672xw/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=2&sn=1a78bf668ae286690969c6f4abb84f93&chksm=0831ec3a3f46652c28c52fb45564530567e692a08b37e5cbe1a9a44721b297d1ebdc7e0d9121#rd', 
            ), 
            2 => array(
              'title' => 'AQMW系列', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19Fapuom2rMEQpbM1s3ZmFHjTGXoiaFMU3PXFNnVsKauOzHYyca5SbOScjTQrPytroYC7T0HTco67w/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=3&sn=daa2530963d22aa1913db3001298106a&chksm=0831ec3a3f46652c1f51a574d92b7467ed0ddd144e201736b52bd54c2a241bfb0e1df900ad5e#rd', 
            ), 
            3 => array(
              'title' => '时光活妍系列', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19Fapuom2rMEQpbM1s3ZmFH8UruCOKvtzW0AqK7zXT6SjdicAMxhiaAvGiaibmjTic7ibibiak7VKkricxHicww/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=4&sn=1d1483f9cf93f6acd4d5bec0c22deb8c&chksm=0831ec3a3f46652c2f6bb06b9354f057bc72209e88a9f21e47de0ba8bcde369e8c21effb7912#rd', 
            ), 
            4 => array(
              'title' => '自然护肤派', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19Fapuom2rMEQpbM1s3ZmFH8m3JRurdssHvNzdSF0l6f8gXhNusgbrtEEAyg7PCOic1FlKDoyA7NVQ/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=5&sn=2175b47fd291831aa792ddd906513a4e&chksm=0831ec3a3f46652ca40c251b04985c661747e12d413f06d37ed4c97868a7a7ffe49c8efc96b8#rd', 
            ), 
            5 => array(
              'title' => '特别保养', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19Fapuom2rMEQpbM1s3ZmFH3ONBCstLGEUjTqA7c8LGD1dhKFqS0Fvmx3ngKxhvia6c4AYXfJPia2zA/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=6&sn=4182db863f6f2a78eef68a321f76d01a&chksm=0831ec3a3f46652cfb51b339c553a24c0f0e79761d2c466d892995152775729f33ecdab43447#rd', 
            ), 
            6 => array(
              'title' => '底妆', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_png/iarUGKvFibG1ibiaiaFpfkyVa6kqkhtx2g43FoD3JoswJTPDxodPpEYwCcDFDNkYkg6KxypfZomibyNvf85Yern3Q5UQ/0?wx_fmt=png', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=7&sn=55cede2f76a19224ea727908300fe2ee&chksm=0831ec3a3f46652c9584b1945f900468a2cf9998bcc6538296b0ea0d98bd3e53766bc2cba86f#rd', 
            ), 
            7 => array(
              'title' => '彩妆', 
              'digest' => '11', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG1ibiaiaFpfkyVa6kqkhtx2g43Fh18zCDjLyqOJXgqfib052lCRxVUYCq9tBb4BmYnsqyUh53mfkXKWoOw/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502438809&idx=8&sn=c445caa7c7ed7ebc0763a032c693292a&chksm=0831ec3a3f46652c6dc07b037d2bc20bc391619f72f406bf9b3dc8aeb1677c0f768e6f80a8d1#rd', 
            ), 
          );
          break;
        case "CLICK_4":
          return array(
            0 => array(
              'title' => '黛珂专柜信息', 
              'digest' => '前往黛珂中国官网或各大专柜。「美的勋章」，等待您的发现。', 
              'cover_url' => 'http://mmbiz.qpic.cn/mmbiz_jpg/iarUGKvFibG19wbN5YGbd8caEg5NAcnAeHVr0FJ6ALdibuvNGVITibrHnWbUkMDXuV50umIgSfib0ROBs4dXnMmJ6qA/0?wx_fmt=jpeg', 
              'content_url' => 'http://mp.weixin.qq.com/s?__biz=MzA4NzU0MDQyOA==&mid=502440438&idx=1&sn=7e7df40afe04770aeed46bbbc25c3773&chksm=0831d6553f465f436728a8357325386557f5e423294442c31e3d5482427a5c06845335a3d98e#rd', 
            ), 
          );
          break;
        default:
          break;
      }
      return null;
    }

    public function getAutoReplyConfig() {
      return array (
        0 => 
        array (
          'reply' => '您好，相关售后问题请联络购买平台客服进行沟通喔~',
          'keywords' => 
          array (
            0 => '售后',
          ),
        ),
        1 => 
        array (
          'reply' => '退货须知一、您在黛珂官网订购的商品，凡在收到货物后发现以下情形的，可以在签收之日起的7天内无条件退货或者换货：
      1、收到的商品发现有过期或者质量问题。2、收到货物时发现商品破损。3、与您在官网上订购的商品不同或有缺少。凡出现上述情形，我们将退回相应的货款并承担因此而产生的运费。
      二、顾客在官网上订购的商品，如因您个人的原因想退货或者换货的，同样可以在签收之日起的7天内申请退换货，但必须满足以下条件：
      1、退货商品须保持未拆封（包括塑封）且未使用的。2、退换的商品须和订单内的发货清单保持一致。3、退换的商品外观不能影响再次销售的。凡出现上述情形，我们将退回相应的货款，但您需要自行承担因此而产生的运费。',
          'keywords' => 
          array (
            0 => '退',
            1 => '退货',
            2 => '怎么退',
            3 => '想要退',
            4 => '不想要',
            5 => '不想买',
          ),
        ),
        2 => 
        array (
          'reply' => '您好，如需办理退款，您需要联系官网在线客户进行取消为您安排退款，一般退款到账时间为一周左右。',
          'keywords' => 
          array (
            0 => '退款',
            1 => '退钱',
          ),
        ),
        3 => 
        array (
          'reply' => '您好，黛珂官网支付方式分为两种：在线支付（包括支付宝、微信、财付通、银联）；货到付款。',
          'keywords' => 
          array (
            0 => '支付',
            1 => '付款',
          ),
        ),
        4 => 
        array (
          'reply' => '您好，点击寻觅黛珂-黛珂官网即可查看产品使用方法哦。',
          'keywords' => 
          array (
            0 => '使用',
            1 => '怎么用',
            2 => '流程',
            3 => '护肤',
            4 => '顺序',
          ),
        ),
        5 => 
        array (
          'reply' => '您好，请问您的肌肤护理有什么需求呢？如果您的肌肤特别容易干燥，较为敏感，可以考虑黛珂AQ系列的护肤产品。如果您的肌肤压力出油、晦暗粗糙，可以考虑黛珂AQMW系列的护肤产品。如果您对肌肤要求较高，可以考虑黛珂AQ珍萃精颜系列。（具体根据您的个人肌肤需求而定，您可前往黛珂专柜咨询选择或直接登录黛珂官方商城了解商品详情。黛珂中国官网地址为：http://cosmedecorte.com.cn/',
          'keywords' => 
          array (
            0 => '年龄',
            1 => '适合',
            2 => '肤质',
            3 => '推荐',
            4 => '好用',
            5 => '哪款',
            6 => '选择',
            7 => '第一次',
            8 => '哪套',
          ),
        ),
        6 => 
        array (
          'reply' => '您好，凡在中国大陆境内黛珂认可专柜和黛珂官网购买任意正价商品（除辅助类产品以外），完整填写《顾客信息记录表》，并接受黛珂会员章程的客人即可成为黛珂珍颜会会员。',
          'keywords' => 
          array (
            0 => '会员',
            1 => '注册',
            2 => '绑定',
            3 => '入会',
            4 => '顾客',
            5 => '信息',
            6 => '记录',
          ),
        ),
        7 => 
        array (
          'reply' => '您好，黛珂天猫官方旗舰店是官方店铺哦，您可放心购买哦。',
          'keywords' => 
          array (
            0 => '天猫',
            1 => '旗舰店',
            2 => '淘宝',
            3 => '网店',
          ),
        ),
        8 => 
        array (
          'reply' => '你好，黛珂目前只有在北京、上海、杭州、南京、太原
        、武汉、天津、成都、沈阳、西安、深圳和哈尔滨有设专柜哦~
      暂时没有开设专柜的城市可以打开黛珂官方微信服务号--寻觅黛珂--在线购买（进入黛珂中国官方商城购买）。',
          'keywords' => 
          array (
            0 => '购买',
            1 => '哪里买',
            2 => '门店',
            3 => '专柜',
            4 => '在线',
            5 => '网上',
            6 => '线下',
            7 => '实体店',
            8 => '店面',
          ),
        ),
        9 => 
        array (
          'reply' => '你好，请在官方指定的渠道购买：黛珂18家线下专柜，以及黛珂官方商城、黛珂官方小程序购买',
          'keywords' => 
          array (
            0 => '真伪',
            1 => '正品',
            2 => '真假',
            3 => '鉴定',
            4 => '批号',
            5 => '查询',
            6 => '假货',
            7 => '防伪',
            8 => '标识',
            9 => '条码',
          ),
        ),
        10 => 
        array (
          'reply' => '你好，黛珂的产品孕妇是可以使用的，但口红以及按摩类的产品建议不要使用。（因为口红可能会吃下去，按摩产品促进血液循环，孕妇慎用）',
          'keywords' => 
          array (
            0 => '孕妇',
            1 => '怀孕',
            2 => '哺乳',
          ),
        ),
        11 => 
        array (
          'reply' => '亲为您带来不便，十分抱歉，您可以拨打黛珂官方客服热线：400-920-2203。',
          'keywords' => 
          array (
            0 => '客服',
            1 => '人工',
            2 => '您好',
            3 => '你好',
            4 => '在吗',
            5 => '有人吗',
            6 => '在',
            7 => '电话',
            8 => '联络',
          ),
        ),
        12 => 
        array (
          'reply' => '你好，您可以打开黛珂官方微信服务号--珍颜会--积分兑礼（登陆后即可进行积分兑礼）。',
          'keywords' => 
          array (
            0 => '兑礼',
            1 => '兑换',
          ),
        ),
        13 => 
        array (
          'reply' => '你好，黛珂官方商城的积分与专柜积分同步，您可以打开黛珂官方微信服务号--珍颜会--积分查询（登录即可查询积分）。',
          'keywords' => 
          array (
            0 => '在黛珂官方商城购买产品可以积分吗？',
            1 => '积分',
          ),
        ),
        14 => 
        array (
          'reply' => '你好，黛珂的产品都是原装进口的哦。',
          'keywords' => 
          array (
            0 => '原装',
            1 => '日本',
            2 => '国产',
            3 => '本土',
          ),
        ),
        15 => 
        array (
          'reply' => '您好，下单后1-2天安排出库，2-6天左右送达，具体以物流配送情况为准。出库后会有短信通知，给到您物流单号。您也可以拨打黛珂官网客服热线：400-920-2203进行查询哦。
      
      ',
          'keywords' => 
          array (
            0 => '发货',
            1 => '订单',
            2 => '快递',
            3 => '物流',
          ),
        ),
        16 => 
        array (
          'reply' => '您好，点击链接取消绑定（http://club.cn-cosmedecorte.com/wx/unbind_card.aspx）',
          'keywords' => 
          array (
            0 => '取消绑定',
          ),
        ),
        17 => 
        array (
          'reply' => '您好，黛珂中国官网地址为：http://cosmedecorte.com.cn/',
          'keywords' => 
          array (
            0 => '官网',
            1 => '黛珂官网',
          ),
        ),
      );
    }

    public function getGreetingMsg($user) {
     return "欢迎来到黛珂！

点击下方菜单，并关注最新推送。
「美的勋章」，等待您的发现。
让黛珂与您一起遇见更好的自己~";
    }

    public function getMenuConfig() {
		$json = $json = <<<EOT
    {
      "button": [
          {
              "name": "珍颜会",
              "sub_button": [
                  {
                      "type": "click",
                      "name": "会员权益",
                      "key": "CLICK_1"
                  },
                  {
                      "type": "view",
                      "name": "会员验证",
                      "url": "http://club.cn-cosmedecorte.com/wx/info.aspx"
                  },
                  {
                      "type": "view",
                      "name": "积分查询",
                      "url": "http://club.cn-cosmedecorte.com/wx/points.aspx"
                  },
                  {
                      "type": "view",
                      "name": "积分兑换",
                      "url": "http://club.cn-cosmedecorte.com/wx/exchange.aspx"
                  }
              ]
          },
          {
              "name": "寻觅黛珂",
              "sub_button": [
                  {
                      "type": "click",
                      "name": "品牌故事",
                      "key": "CLICK_2"
                  },
                  {
                      "type": "click",
                      "name": "热荐臻品",
                      "key": "CLICK_3"
                  },
                  {
                      "type": "click",
                      "name": "专柜信息",
                      "key": "CLICK_4"
                  },
                  {
                      "type": "view",
                      "name": "品牌官网",
                      "url": "http://cosmedecorte.com.cn/"
                  },
                  {
                      "type": "miniprogram",
                      "name": "在线购买",
                      "url": "https://shop.decorte-cosmetics.cn",
                      "appid": "wxe4f65bfc49947618",
                      "pagepath": "pages/index/index"
                  }
              ]
          },
          {
              "name": "至臻体验",
              "sub_button": [
                  {
                      "type": "view",
                      "name": "美颜预约",
                      "url": "http://salon.cn-cosmedecorte.com/beauty_chance.aspx"
                  }
              ]
          }
      ]
  }
EOT;
		return $json;
    }
}