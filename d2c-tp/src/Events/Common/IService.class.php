<?php
namespace Events\Common;

interface IService
{
    public function onUserCreate($uid, $eid);
    public function onRegister($uid, $eid);
    public function getEnjoyAmount($uid, $eid, $code, $content);
    public function onEnjoy($uid, $eid, $item_id, $code, $content);
    public function onPlay($uid, $eid, $item_id, $code, $content);
    public function getTicketUrl($ticket_url, $code);
    public function getStatData($eid);
    public function onRegisterDetail($uid, $eid);
    public function onAuth($uid, $eid, $item_id, $code, $content);
    public function filterItemContent($uid, $eid, $item_id, $code, $content);
}
