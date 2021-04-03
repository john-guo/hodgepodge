<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IWechat;

class Run201903_WechatService extends \Events\Common\WechatService implements IWechat
{
    const POPUP_TITLE = '快来获取三菱电机 RUN FOR ECO 环跑小达人写真照吧！';
    const POPUP_CONTENT = '快来看我的环跑比赛好成绩吧！';
    const POPUP_PIC = 'https://n.d2c-china.cn/images/runforeco.jpg';
    const POPUP_URL = 'https://n.d2c-china.cn/event/run2019/index.html';

    public function onUserScan($user, $code) {
      if (empty($code))
          return null;

      $content = $this->getTicketContent($code);
      if (empty($content)) {
        $this->weixinlog('empty content', 'scan_result', $code, '', $user);
        return null;
      }

      $url = self::POPUP_URL . '?url=' . urlencode($content);

      $this->weixinlog($url, 'scan_result', $code, '', $user);

      return 
      array (
          array(
              'title' =>  self::POPUP_TITLE,
              'digest' => self::POPUP_CONTENT,
              'cover_url' => self::POPUP_PIC,
              'content_url' => $url,
          )
      );
    }

  public function getGreetingMsg($user) {
      $msg = <<<EOT
感谢您对三菱电机中国官方微信的支持。在这里，您将看到三菱电机在中国以及全球领域的最新科技，企业动态，社会贡献活动等。
期待您的持续关注和支持！欲知详情，点击如下分类信息：
■<a href="http://cn.mitsubishielectric.com/zh/products/index.page">产品介绍</a>
■<a href="http://cn.mitsubishielectric.com/zh/contact-us/index.page">产品咨询，售后服务相关</a>
■<a href="http://cn.mitsubishielectric.com/zh/contact-us/tel-list/index.page">三菱电机中国集团公司一览表</a>
■<a href="http://cn.mitsubishielectric.com/zh/contact-us/index.page">模仿品相关咨询</a>
■<a href="http://cn.mitsubishielectric.com/zh/about-us/local/corporate-activities/index.page">看到不一样的三菱电机</a> 
■<a href="http://cn.mitsubishielectric.com/zh/news-events/index.page">三菱电机最新新闻和活动</a>
■<a href="http://cn.mitsubishielectric.com/zh/index.page">三菱电机更多信息</a>
EOT;
    return $msg;
  }


  public function getAutoReplyConfig() {
    return 
    array (
      0 => 
      array (
        'reply' => '欢迎参加活动',
        'keywords' => 
        array (
          0 => '环跑',
        ),
      ),
      1 => 
      array (
        'reply' => '您好，您的信息小菱已经收到啦。有问题要咨询吗？点击下面这个链接，输入相关信息，小菱的小伙伴会尽快回复您哟！
http://cn.mitsubishielectric.com/zh/contact-us/index.page#h0101',
        'keywords' => 
        array (
          0 => '维修',
        ),
      ),
      2 => 
      array (
        'reply' => '您好，您的信息小菱已经收到啦。有问题要咨询吗？点击下面这个链接，输入相关信息，小菱的小伙伴会尽快回复您哟！
http://cn.mitsubishielectric.com/zh/contact-us/index.page#h0101',
        'keywords' => 
        array (
          0 => '投诉',
          1 => '维护',
          2 => '空调',
          3 => '售后',
          4 => '客服',
          5 => '人工',
          6 => '空调',
          7 => '疑问',
          8 => '问题',
          9 => '报修',
        ),
      ),
    );
  }

  public function getMenuConfig() {
    $json = <<<EOT
{"button":[{"name":"环跑活动","sub_button":[{"type":"view","name":"活动概要","url":"http://cn.mitsubishielectric.com/zh/about-us/local/csr/social-welfare/index.page"},{"type":"view","name":"历届活动回顾","url":"https://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=6&sn=161941ad6a520df038c626ba3405d477&scene=18"}]},{"name":"产品技术","sub_button":[{"type":"view","name":"产品介绍","url":"http://cn.mitsubishielectric.com/zh/products/index.page"},{"type":"view","name":"解决方案","url":"http://cn.mitsubishielectric.com/zh/solution/index.page"},{"type":"view","name":"产品问询","url":"http://cn.mitsubishielectric.com/zh/contact-us/index.page"},{"type":"view","name":"模仿品问询","url":"http://cn.mitsubishielectric.com/zh/contact-us/index.page"},{"type":"view","name":"供应商窗口","url":"https://www.mitsubishielectric.com/contact/ssl/php/1056/inquiryform.php?fid=1056&kind=supply"}]},{"name":"小菱热推","sub_button":[{"type":"view","name":"产品技术秀","url":"http://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=2&sn=a60bf8a5526ffe4bab7cc47d164b7f90&scene=18#wechat_redirect"},{"type":"view","name":"CSR进行时","url":"http://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=3&sn=1eba6395d8043bc35fd04f318a84b718&scene=18#wechat_redirect"},{"type":"view","name":"精彩新闻","url":"http://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=4&sn=9a8f9408ad240123540b2d9faec6e3a8&scene=18#wechat_redirect"},{"type":"view","name":"海外视点","url":"http://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=1&sn=10bbff2eca52a4ef067231186e851c0e&scene=18#wechat_redirect"},{"type":"view","name":"小菱海报","url":"http://mp.weixin.qq.com/mp/homepage?__biz=MzIyNjQ5Njg4Nw==&hid=5&sn=cf96cb1bf32176bac0968b88c3365d89&scene=18#wechat_redirect"}]}]}
EOT;
    return $json;
  }
}