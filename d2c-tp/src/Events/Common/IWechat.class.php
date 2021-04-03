<?php
namespace Events\Common;

interface IWechat
{
    public function onUserScan($user, $code);
    public function onMenuClick($key);
    public function getAutoReplyConfig(); 
    public function getGreetingMsg($user);
    public function getMenuConfig();
}
