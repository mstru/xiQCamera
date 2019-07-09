# XIQCAMERA - WPF aplik�cia
Tento repozit�r obsahuje vytvorenie WPF aplik�cie za pomoci technol�gi�:

    a. C#,
    b. WPF
    c. xiAPI.dll

### XIQCAMERA aplik�cia umo��uje:

 Z�lo�ka Camera:

    1. Inicializ�ciu inteligetnej Ximea kamery tla�idlo CONNECT,
    2. Spustenie a zastavenie sn�mkovania (akviz�cie) tla�idlo START/STOP,
    3. Ulo�enie nastaven� tla�idlo SAVE a na��tanie posledn�ch,
    4. Reinicializ�cia kamery tla�idlom RE-CONNECT,			
    2. Nastavenie z�kladn�ch parametrov (Exposure, FPS, Gain) cez rozhranie API,
    3. Zistenie aktu�lnej mo�nej povolenej hodnoty a jej zobrazenie (Exposure, FPS, Gain),
    4. Sledovanie v�konu (Performance): 
	a. Update - r�chlos� zmeny nastaven� cez API [ms] 
	b. Aquisition - r�chlos� sn�mkovania [ms]
	c. Write - z�pis na disk [ms]
	d. Total - celkov� �as [ms]
    5. Po��tadlo sn�mkov (Images)

 Z�lo�ka Images:

    1. Grid - zobrazenie vytvoren�ch sn�mkov a automatick� refresh grid-u	
	    
![Alt text](/XIQCAMERA.jpg?raw=true "Optional Title")