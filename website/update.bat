copy app_update app_offline.htm
cd ..
"%programfiles(x86)%\Git\cmd\git" pull https://SyncUser:PULL1repo@github.com/JesseJC/SaveStateHeroes.git
sleep 2
DEL website\app_offline.htm