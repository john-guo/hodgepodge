<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IService;

class Kose201903_Service extends \Events\Logic\DefaultService
{
    use \Events\Logic\EventService;

    const CODE_REGISTER = "REG";
    const CODE_CAMERA = "CAM";
    const CODE_PRINT = "PRN";

    public function onUserCreate($uid, $eid) 
    {
        $this->_enjoy($uid, self::CODE_REGISTER, '');
    }

    public function onEnjoy($uid, $eid, $item_id, $code, $content)
    {
        if ($code === self::CODE_REGISTER) {
            $this->_play($uid, $eid, $item_id);
        }
    }

    public function onPlay($uid, $eid, $item_id, $code, $content) 
    {
        // if ($code === self::CODE_CAMERA) {
        //     $this->_enjoy($uid, self::CODE_PRINT, $content);
        // }
    }

    public function onAuth($uid, $eid, $item_id, $code, $content)
    {
        if ($code === self::CODE_CAMERA) {
            $json = json_decode($content, true);
            $content = $json['print'];
            $this->_enjoy($uid, self::CODE_PRINT, $content);
        }
    }

    public function filterItemContent($uid, $eid, $item_id, $code, $content)
    {
        switch ($code) {
            case self::CODE_CAMERA:
                $json = json_decode($content, true);
                if (empty($json))
                    return $content;
                return $json['square'];
            break;
            default:
            break;
        }

        return $content;
    }

    public function getStatData($eid) 
    {
        $sql = $this->getEmptyModel();
        $result = $sql->query("SELECT date(create_ts), item_id, code, name, count(*) as amount from user_items where event_id = $eid and status=1 group by date(create_ts), item_id;");
        $data['details'] = $result;

        $result = $sql->query("select item, count(*) as amount from (select user_id, count(item_id) as item from user_items where event_id=$eid and status=1 group by user_id) a group by item;");
        $data['info'] = $result;
        
        return $data;
    }
}