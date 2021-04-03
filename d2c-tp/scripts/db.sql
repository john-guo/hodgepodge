SET FOREIGN_KEY_CHECKS=0;


-- ----------------------------
-- Table structure for `weixin_logs`
-- ----------------------------
DROP TABLE IF EXISTS `weixin_logs`;
CREATE TABLE `weixin_logs` (
`id`  int(11) NOT NULL AUTO_INCREMENT ,
`create_ts`  timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ,
`action`  varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL ,
`memo`  text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL ,
`type`  varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL ,
`event`  varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL ,
`from`  varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL ,
`to`  varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL ,
PRIMARY KEY (`id`)
)
ENGINE=MyISAM
DEFAULT CHARACTER SET=utf8mb4 COLLATE=utf8mb4_general_ci
;
-- ----------------------------
-- Table structure for `logs`
-- ----------------------------
DROP TABLE IF EXISTS `logs`;
CREATE TABLE `logs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `operator_id` int(11) NOT NULL,
  `op` varchar(100) DEFAULT NULL,
  `name` varchar(100) DEFAULT NULL,
  `target` varchar(100) DEFAULT NULL,
  `memo` text,
  PRIMARY KEY (`id`)
)
ENGINE=MyISAM
DEFAULT CHARACTER SET=utf8mb4 COLLATE=utf8mb4_general_ci
;


CREATE TABLE `backdoor_logs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `create_ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `operator` varchar(100) DEFAULT NULL,
  `type` varchar(20) DEFAULT NULL,
  `ip` varchar(20) DEFAULT NULL,
  `target` varchar(50) DEFAULT NULL,
  `memo` text,
  PRIMARY KEY (`id`)
)
ENGINE=MyISAM
DEFAULT CHARACTER SET=utf8mb4 COLLATE=utf8mb4_general_ci
;
