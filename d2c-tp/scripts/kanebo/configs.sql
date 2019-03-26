/*
Navicat MySQL Data Transfer

Source Server         : 112.124.5.198
Source Server Version : 50556
Source Host           : localhost:3306
Source Database       : db_kanebo

Target Server Type    : MYSQL
Target Server Version : 50556
File Encoding         : 65001

Date: 2019-01-14 18:23:04
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `configs`
-- ----------------------------
DROP TABLE IF EXISTS `configs`;
CREATE TABLE `configs` (
  `cfg_key` varchar(50) NOT NULL,
  `cfg_value` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`cfg_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ----------------------------
-- Records of configs
-- ----------------------------
INSERT INTO `configs` VALUES ('game_1', '0');
INSERT INTO `configs` VALUES ('game_2', '0');
