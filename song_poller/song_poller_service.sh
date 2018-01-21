#! /bin/sh
### BEGIN INIT INFO
# Provides:          song_poller
# Required-Start:    $all
# Required-Stop:
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Grabs messages from an aws sqs queue
### END INIT INFO

#cp song_poller_service.sh /etc/init.d/
#chmod u+x /etc/init.d/song_poller_service.sh
#update-rc.d /etc/init.d/song_poller_service.sh defaults
#OR
#update-rc.local /etc/init.d/song_poller_service.sh defaults


PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/opt/bin

. /lib/init/vars.sh
. /lib/lsb/init-functions
# If you need to source some other scripts, do it here

song_poller_script_path=/home/pi/jukebox/song_poller/__init__.py

case "$1" in
  start)
    log_begin_msg "Starting Jukebox Song Poller Service"
    sleep 60
    cd /home/pi/jukebox
    git fetch origin
    git reset --hard origin/master
    chown pi:pi -R /home/pi/jukebox/song_poller/
    cp /home/pi/jukebox/song_poller/song_poller_service.sh /etc/init.d/
    chmod u+x /etc/init.d/song_poller_service.sh
    update-rc.d song_poller_service.sh defaults
    pip install -r /home/pi/jukebox/song_poller/requirements.txt -U
    python ${song_poller_script_path}
# do something
    log_end_msg $?
    exit 0
    ;;
  stop)
    log_begin_msg "Stopping Jukebox Song Poller Service"
    pkill -f ${song_poller_script_path}
    log_end_msg $?
    exit 0
    ;;
  *)
    echo "Usage: service song_poller {start|stop}"
    exit 1
    ;;
esac

# Fix cloudformation policy for sqs queue
