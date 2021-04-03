<?php
namespace Events\Common;

interface IBackdoor
{
    public function play($event_id, $item_id, $uid);
}