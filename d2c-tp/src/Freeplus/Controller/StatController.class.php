<?php
namespace Freeplus\Controller;
use \Common\Common\Constant;
use \Common\Common\Utils;
class StatController extends \Common\Controller\ApiController {
    public function index() {
        $stat = $this->getModel("users");
        $count1 = $stat->where(['gift_1' => 1])->count();
        $count2 = $stat->where(['gift_2' => 1])->count();
        echo <<<EOT
        <!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
</head>
<body>
EOT;

        echo "领中样人数: $count1<br/>";
        echo "领优惠券人数: $count2<br/>";

        $stat = $this->getEmptyModel();
        $result = $stat->query("SELECT date(create_ts) as date, sum(gift_1) as count1, sum(gift_2) as count2 FROM users group by date");
        echo <<<EOT
		每日详情<br/>
		<table border='1'>
			<thead>
				<tr>
				<th>日期</th>
				<th>领中样人数</th>
				<th>领优惠券人数</th>
				</tr>
			</thead>
			<tbody>
EOT;
		foreach ($result as $item) {
            echo "<tr>
                    <td>{$item['date']}</td>
                    <td>{$item['count1']}</td>
                    <td>{$item['count2']}</td>
                </tr>";
		}
		echo <<<EOT
			</tbody>
		  </table>
EOT;

        echo <<<EOT
        </body>
        </html>
EOT;
    }
}