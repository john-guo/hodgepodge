<?php
namespace Events\Logic;
use \Common\Common\Constant;
use \Common\Common\Utils;
use \Events\Common\IService;

class Shiseido201903II_Service extends \Events\Logic\DefaultService
{
    use \Events\Logic\EventService;

    const CODE_REWARD = "003";

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
        1 => [
            "2019-03-13"=>10,
            "2019-03-16"=>600,
            "2019-03-17"=>600,
            "2019-03-18"=>300,
            "2019-03-19"=>300,
            "2019-03-20"=>300,
            "2019-03-21"=>300,
            "2019-03-22"=>300,
            "2019-03-23"=>600,
            "2019-03-24"=>600,
            
        ],
        2 =>[
            "2019-03-15"=>100,
            "2019-03-16"=>250,
            "2019-03-17"=>250,
            "2019-03-18"=>100,
            "2019-03-19"=>100,
            "2019-03-20"=>100,
            "2019-03-21"=>100,
            "2019-03-22"=>100,
            "2019-03-23"=>250,
            "2019-03-24"=>250,
            "2019-03-25"=>100,
            "2019-03-26"=>100,
            "2019-03-27"=>100,
            "2019-03-28"=>100,
            "2019-03-29"=>100,
            "2019-03-30"=>250,
            "2019-03-31"=>250,
            
        ],
        3 =>[
            "2019-04-08"=>300,
            "2019-04-09"=>300,
            "2019-04-10"=>300,
            "2019-04-11"=>300,
            "2019-04-12"=>300,
            "2019-04-13"=>600,
            "2019-04-14"=>600,
            
        ],
        4 =>[
            "2019-04-01"=>250,
            "2019-04-02"=>250,
            "2019-04-03"=>250,
            "2019-04-04"=>250,
            "2019-04-05"=>400,
            "2019-04-06"=>400,
            "2019-04-07"=>400,
            
        ],
        5 =>[
            "2019-04-18"=>250,
            "2019-04-19"=>250,
            "2019-04-20"=>350,
            "2019-04-21"=>350,
            "2019-04-22"=>250,
            "2019-04-23"=>250,
            "2019-04-24"=>250,
            
        ],
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
        $statModel->startTrans();
        $stat = $statModel->lock(true)->where($cond)->find();
        $this->checkDb($stat, function() use($statModel) {
            $statModel->rollback();
        });

        $statCount = $stat['stat_count'];
        $update_ts = $stat['update_ts'];
        $stat_dt = Utils::getDate($update_ts);

        if (empty($stat)) {
            $ret =  $statModel->add($cond);
            $this->checkDb($ret, function() use($statModel) {
				$statModel->rollback();
			});
            $statCount = 0;
            $stat_dt = Utils::today();
        }

        $today = Utils::today();
        if ($stat_dt != $today) {
            $statCount = 0;
        }

        $statCount++;

        $citycfg = self::$REWARD_SHOPS[$city];
        if (empty($citycfg)) {
            $statModel->rollback();
            $this->result(Constant::ERROR_USER_CITY_NOTFOUND);
        }

        $max = $citycfg[$stat_dt];
        if (empty($max)) {
            $statModel->rollback();
            $this->result(Constant::ERROR_USER_CITY_NOTFOUND);
        }

        $ret = $statModel->where($cond)->setField([ 'stat_count' => $statCount ]);
        $this->checkDb($ret , function() use($statModel) {
            $statModel->rollback();
        });

        $statModel->commit();

        if ($statCount <= $max) {
            if ($statCount % 10 == 0) 
            {
                return self::REWARD_1;
            }

            if ($statCount % 2 == 0)
            {
                return self::REWARD_2;
            }

            return self::REWARD_3;
        }

        // $statModel->rollback();
        // $this->result(Constant::ERROR_USER_FULL_REWARD);
      
        return 0;
    }

    public function onPlay($uid, $eid, $item_id, $code, $content) 
    {
        if ($code !== self::CODE_REWARD) {
            return;
        }

        $this->_settle($uid);
    }

    public function onRegisterDetail($uid, $eid) {
        $itemcfg = $this->getItem($eid, self::CODE_REWARD);
        $this->_play($uid, $eid, $itemcfg['id']);
    }

    public function getStatData($eid) 
    {
        return null;
    }
}