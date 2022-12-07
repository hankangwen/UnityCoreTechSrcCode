@echo off  
:begin
@echo ----------1, create all game database------------ 
mysql -uroot -p123456<createdb.sql
mysql -uroot -p123456<grantuser.sql 

mysql -uroot -p123456<fball_accountdb.sql
mysql -uroot -p123456<fball_chargedb.sql
mysql -uroot -p123456<fball_robedb.sql

mysql -uroot -p123456 fball_accountdb < fball_accountdb.sql
mysql -uroot -p123456 fball_chargedb < fball_chargedb.sql
mysql -uroot -p123456 fball_robedb < fball_robedb.sql

mysql -uroot -p123456 fball_gamedb_1 < fball_gamedb.sql
mysql -uroot -p123456 fball_gamedb_2 < fball_gamedb.sql
mysql -uroot -p123456 fball_gamedb_3 < fball_gamedb.sql 

mysql -uroot -p123456 fball_logdb_1 < fball_logdb.sql
mysql -uroot -p123456 fball_logdb_2 < fball_logdb.sql
mysql -uroot -p123456 fball_logdb_3 < fball_logdb.sql 

::mysql -uroot -p123456 fball_gamedb_2 < pr_del_time_over_mail.sql
::mysql -uroot -p123456 fball_gamedb_3 < pr_del_time_over_mail.sql

::mysql -uroot -p123456 fball_gamedb_3 < event_del_time_over_mail.sql
::mysql -uroot -p123456 fball_gamedb_3 < event_del_time_over_mail.sql
::mysql -uroot -p123456 fball_gamedb_3 < event_del_time_over_mail.sql

@echo ----------create all game database ok!------------ 
@echo ================================================== 
@echo ----------2, start create gamelog folder----------
::如需要修改，则修改这里的文件夹路径	
set logpath="d:\zhyz\gamelog"   
md %logpath%
@echo  ----------create gamelog folder ok---------- 
::创建数据库备份路径,如需修改，则修改这里
set backdbpath="d:\zhyz\dbback" 
md %backdbpath%
@echo  ----------create dbback folder ok---------- 
::创建数据库备份计划任务
call schedule_db_back.bat
	::pause
	::end