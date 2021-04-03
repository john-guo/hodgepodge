/*
Navicat MySQL Data Transfer

Source Server         : 112.124.5.198
Source Server Version : 50556
Source Host           : localhost:3306
Source Database       : db_kanebo

Target Server Type    : MYSQL
Target Server Version : 50556
File Encoding         : 65001

Date: 2019-01-07 15:23:50
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `users`
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` datetime NOT NULL,
  `update_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `open_id` varchar(100) DEFAULT NULL,
  `nickname` varchar(100) DEFAULT NULL,
  `header` text,
  `score_1` int(4) NOT NULL DEFAULT '0',
  `score_2` int(4) NOT NULL DEFAULT '0',
  `status` tinyint(4) NOT NULL DEFAULT '0',
  `score_time_1` datetime DEFAULT NULL,
  `score_time_2` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_openid` (`open_id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2841 DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Records of users
-- ----------------------------
