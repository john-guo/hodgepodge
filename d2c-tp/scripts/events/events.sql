/*
Navicat MySQL Data Transfer

Source Server         : 112.124.5.198
Source Server Version : 50556
Source Host           : localhost:3306
Source Database       : db_events

Target Server Type    : MYSQL
Target Server Version : 50556
File Encoding         : 65001

Date: 2019-03-13 10:52:35
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `admins`
-- ----------------------------
DROP TABLE IF EXISTS `admins`;
CREATE TABLE `admins` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `account` varchar(100) NOT NULL,
  `password` varchar(100) NOT NULL,
  `level` int(11) NOT NULL DEFAULT '0',
  `last_login_time` datetime DEFAULT NULL,
  `last_login_ip` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_admin` (`account`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `event_items`
-- ----------------------------
DROP TABLE IF EXISTS `event_items`;
CREATE TABLE `event_items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `event_id` int(11) NOT NULL,
  `type` tinyint(4) NOT NULL DEFAULT '0',
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `name` varchar(100) DEFAULT NULL,
  `code` varchar(100) NOT NULL,
  `secure` tinyint(4) NOT NULL DEFAULT '0',
  `allow_change` tinyint(4) NOT NULL DEFAULT '0',
  `allow_push` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_code` (`event_id`,`code`),
  KEY `idx_event` (`event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `event_miniprogs`
-- ----------------------------
DROP TABLE IF EXISTS `event_miniprogs`;
CREATE TABLE `event_miniprogs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `event_id` int(11) NOT NULL,
  `app_id` varchar(100) DEFAULT NULL,
  `appsecret` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_event` (`event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `event_stats`
-- ----------------------------
DROP TABLE IF EXISTS `event_stats`;
CREATE TABLE `event_stats` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `event_id` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `stat_key` varchar(100) NOT NULL,
  `stat_count` int(11) NOT NULL DEFAULT '0',
  `stat_value` int(11) NOT NULL DEFAULT '0',
  `stat_memo` text,
  `stat_value2` int(11) NOT NULL DEFAULT '0',
  `stat_value3` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_stat` (`event_id`,`item_id`,`stat_key`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=74 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `event_tickets`
-- ----------------------------
DROP TABLE IF EXISTS `event_tickets`;
CREATE TABLE `event_tickets` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `event_id` int(11) NOT NULL,
  `content` text,
  PRIMARY KEY (`id`),
  KEY `idx_event` (`event_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `event_wechats`
-- ----------------------------
DROP TABLE IF EXISTS `event_wechats`;
CREATE TABLE `event_wechats` (
  `id` int(11) NOT NULL,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `appid` varchar(100) DEFAULT NULL,
  `appsecret` varchar(100) DEFAULT NULL,
  `aeskey` varchar(100) DEFAULT NULL,
  `token` varchar(100) DEFAULT NULL,
  `class_name` varchar(100) DEFAULT NULL,
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `boot_url` varchar(200) DEFAULT NULL,
  `scan_url` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `events`
-- ----------------------------
DROP TABLE IF EXISTS `events`;
CREATE TABLE `events` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `name` varchar(100) DEFAULT NULL,
  `start_time` datetime DEFAULT NULL,
  `end_time` datetime DEFAULT NULL,
  `item_count` int(11) DEFAULT NULL,
  `class_name` varchar(200) DEFAULT NULL,
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `ticket_url` varchar(200) DEFAULT NULL,
  `settle_url` varchar(200) DEFAULT NULL,
  `service_class` varchar(200) DEFAULT NULL,
  `api_key` varchar(100) DEFAULT NULL,
  `sms_token` varchar(100) DEFAULT NULL,
  `need_userinfo` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `user_event_items`
-- ----------------------------
DROP TABLE IF EXISTS `user_event_items`;
CREATE TABLE `user_event_items` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `user_id` int(11) NOT NULL,
  `event_id` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `content` text,
  `amount` int(11) NOT NULL DEFAULT '0',
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `pstatus` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_items` (`user_id`,`event_id`,`item_id`),
  KEY `idx_events` (`event_id`),
  KEY `idx_user_events` (`user_id`,`event_id`),
  KEY `idx_event_items` (`event_id`,`item_id`,`pstatus`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=11272 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Table structure for `users`
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `open_id` varchar(100) DEFAULT NULL,
  `mobile` varchar(100) DEFAULT NULL,
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `event_id` int(11) NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  `address` varchar(200) DEFAULT NULL,
  `gender` varchar(10) DEFAULT NULL,
  `birthday` varchar(100) DEFAULT NULL,
  `city` varchar(100) DEFAULT NULL,
  `digest` text,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_user` (`mobile`,`event_id`),
  UNIQUE KEY `idx_open_id` (`open_id`,`event_id`),
  KEY `idx_event` (`event_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8116 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- View structure for `user_items`
-- ----------------------------
DROP VIEW IF EXISTS `user_items`;
CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY INVOKER VIEW `user_items` AS select `event_items`.`code` AS `code`,`event_items`.`name` AS `name`,`event_items`.`type` AS `type`,`event_items`.`secure` AS `secure`,`event_items`.`allow_push` AS `allow_push`,`event_items`.`allow_change` AS `allow_change`,`user_event_items`.`pstatus` AS `pstatus`,`user_event_items`.`status` AS `status`,`user_event_items`.`amount` AS `amount`,`user_event_items`.`content` AS `content`,`user_event_items`.`item_id` AS `item_id`,`user_event_items`.`event_id` AS `event_id`,`user_event_items`.`user_id` AS `user_id`,`user_event_items`.`update_ts` AS `update_ts`,`user_event_items`.`create_ts` AS `create_ts`,`user_event_items`.`id` AS `id`,`event_items`.`status` AS `item_status`,`users`.`mobile` AS `mobile`,`users`.`open_id` AS `open_id`,`users`.`name` AS `username`,`users`.`city` AS `city`,`users`.`address` AS `address`,`users`.`gender` AS `gender`,`users`.`birthday` AS `birthday`,`users`.`digest` AS `digest` from ((`event_items` join `user_event_items` on(((`event_items`.`event_id` = `user_event_items`.`event_id`) and (`user_event_items`.`item_id` = `event_items`.`id`)))) join `users` on((`user_event_items`.`user_id` = `users`.`id`))) ;
