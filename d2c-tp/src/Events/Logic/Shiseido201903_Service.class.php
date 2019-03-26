<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IService;

class Shiseido201903_Service extends \Events\Logic\DefaultService
{
    use \Events\Logic\EventService;

    const CODE_REWARD = "001";

    const REWARD_1 = 1;
    const REWARD_2 = 2;
    const REWARD_3 = 3;

    // const MAX_REWARD_1 = 1;
    // const MAX_REWARD_2 = 2;
    // const MAX_REWARD_3 = 3;

    const REWARD_P1 = 71;
    const REWARD_P2 = 5;
    const REWARD_P2_TOTAL = 15;
    // const REWARD_P3 = 3;
    // const REWARD_P_BASE = 10000;

    static $REWARD_SHOPS = [
        '00100171',
        '01700014',
        '10100077',
        '00100014',
        '00100134',
        '00100160',
        '00100191',
        '00100208',
        '02100004',
        '02100022',
        '02200012',
        '10100113',
        '10100120',
        '10100183',
        '10200018',
        '10200034',
        '00300004',
        '00300036',
        '00300099',
        '00300123',
        '00800039',
        '01000030',
        '10100157',
        '10100202',
        '10100211',
        '00300066',
        '00400025',
        '00400029',
        '01200021',
        '01300015',
        '01300019',
        '02000003',
        '10100019',
        '10100033',
        '10100048',
        '10100051',
        '10100129',
        '10100130',
        '10100134',
        '10100141',
        '10100143',
        '10100154',
        '10100165',
        '10100176',
        '10100190',
        '10100195',
        '01400029',
        '01400057',
        '10100153',
        '10100177',
    ];

    public function getEnjoyAmount($uid, $eid, $code, $content) 
    {
        if ($code !== self::CODE_REWARD) {
            return 0;
        }

        $user = $this->getUser($uid);
        $city = $user['city'];
        $itemcfg = $this->getItem($eid, $code);
        $id = $itemcfg['id'];

        $statModel = $this->getModel("event_stats");
        $cond = [ 'event_id' => $eid, 'item_id' => $id, 'stat_key' => $city ];
        $stat = $statModel->where($cond)->find();
        $this->checkDb($stat);

        $statCount = $stat['stat_count'];
        $statValue = $stat['stat_value'];
        $statMemo = $stat['stat_memo'];
        $update_ts = $stat['update_ts'];
        $statValue2 = $stat['stat_value2'];
        $stat_dt = Utils::getDate($update_ts);

        if (empty($stat)) {
            $ret =  $statModel->add($cond);
            $this->checkDb($ret);
            $statCount = 0;
            $statValue = 0;
            $statMemo = '';
            $stat_dt = Utils::today();
        }

        if (in_array($city, self::$REWARD_SHOPS) && empty($statMemo) && $statCount + 1 >= self::REWARD_P1) 
        {
            $ret = $statModel->where($cond + [ 'stat_count' => $statCount, 'stat_memo' => ['exp', 'is null'] ])
                    ->setField(['stat_count' => $statCount + 1, 'stat_memo' => '1']);
            $this->checkDb($ret);

            if (!empty($ret)) {
                return self::REWARD_1;
            }
        }

        $today = Utils::today();
        if ($stat_dt != $today) {
            $statValue2 = 0;
        }

        if ($statValue + 1 >= self::REWARD_P2 && $statValue2 < self::REWARD_P2_TOTAL)
        {
            $ret = $statModel->where($cond + [ 'stat_value' => $statValue])->setField(['stat_value' => 0, 'stat_value2' => $statValue2 + 1, 'stat_count' => ['exp', 'stat_count+1']]);
            $this->checkDb($ret);

            if (!empty($ret)) {
                return self::REWARD_2;
            }
        }

        $ret = $statModel->where($cond)->setField(['stat_count' => ['exp', 'stat_count+1'], 'stat_value' => ['exp', 'stat_value+1']]);
        $this->checkDb($ret);

        return self::REWARD_3;

        // $sql = $this->getEmptyModel();
        // $result = $sql->query("SELECT amount, count(*) as c from user_event_items where event_id = $eid and item_id = $id group by amount;");
        // $this->checkDb($result);
        // array_column($result, 'c', 'amount');

        // $r = rand(1, self::REWARD_P_BASE);

        // if ($r <= self::REWARD_P1 && $result[self::REWARD_1] < MAX_REWARD_1) {
        //     $this->_play($uid, $eid, $id);
        //     return self::REWARD_1;
        // }

        // if ($r <= self::REWARD_P2 && $result[self::REWARD_2] < MAX_REWARD_2) {
        //     return self::REWARD_2;
        // }

        // if ($r <= self::REWARD_P3 && $result[self::REWARD_3] < MAX_REWARD_3) {
        //     return self::REWARD_3;
        // }

        // return 0;
    }

    public function onPlay($uid, $eid, $item_id, $code, $content) 
    {
        if ($code !== self::CODE_REWARD) {
            return;
        }

        $this->_settle($uid);
    }

    public function getTicketUrl($ticket_url, $code) 
    {
        return $code;
    }

    public function onRegisterDetail($uid, $eid) {
        $itemcfg = $this->getItem($eid, self::CODE_REWARD);
        $this->_play($uid, $eid, $itemcfg['id']);
    }

    public function getStatData($eid) 
    {
        $sql = $this->getEmptyModel();
        $result = $sql->query("SELECT city, address, item_id, code, name, count(*) as amount from user_items where event_id = $eid group by city, code;");
        $data['info'] = $result;

        $result = $sql->query("SELECT date(create_ts) as date, city, address, item_id, code, name, count(*) as amount from user_items where event_id = $eid group by date(create_ts), city, code;");
        $data['details'] = $result;

        $code = self::CODE_REWARD;
        $result = $sql->query("SELECT username, mobile, city, address, digest, status from user_items where event_id = $eid and code='$code' and amount = 1;");
        $data['one'] = $result;

        $result = $sql->query("SELECT status, count(*) as amount from user_items where event_id = $eid and code='$code' and amount = 2 group by status;");
        $data['two']['info'] = $result;

        $result = $sql->query("SELECT status, count(*) as amount from user_items where event_id = $eid and code='$code' and amount = 3 group by status;");
        $data['three']['info'] = $result;

        $result = $sql->query("SELECT city, address, status, count(*) as amount from user_items where event_id = $eid and code='$code' and amount = 2 group by city, status;");
        $data['two']['details'] = $result;

        $result = $sql->query("SELECT city, address, status, count(*) as amount from user_items where event_id = $eid and code='$code' and amount = 3 group by city, status;");
        $data['three']['details'] = $result;

        return $data;
    }
}