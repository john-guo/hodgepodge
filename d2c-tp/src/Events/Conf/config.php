<?php
return array(
    'URL_MODEL' => 2,
    'HTTP_CACHE_CONTROL' => 'no-store,no-cache',
    'SESSION_AUTO_START' => true,
    'DB_TYPE'   => 'mysql', // 数据库类型
    'DB_HOST'   => 'localhost', // 服务器地址
    'DB_NAME'   => 'db_events', // 数据库名
    'DB_USER'   => 'root', // 用户名
    'DB_PWD'    => 'd2cChin@', // 密码
    'DB_PORT'   => 3306, // 端口
    'DB_PARAMS' =>  array(PDO::ATTR_PERSISTENT => true), // 数据库连接参数
    'DB_PREFIX' => '', // 数据库表前缀 
    'DB_CHARSET'=> 'utf8mb4', // 字符集
    'DB_DEBUG'  =>  FALSE, // 数据库调试模式 开启后可以记录SQL日志
    'DB_BIND_PARAM' => true,
 );