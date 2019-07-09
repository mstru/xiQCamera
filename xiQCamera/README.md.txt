# XIQCAMERA - WPF aplikácia
Tento repozitár obsahuje vytvorenie WPF aplikácie za pomoci technológií:

    a. C#,
    b. WPF
    c. xiAPI.dll

### XIQCAMERA aplikácia umoòuje:

 Záloka Camera:

    1. Inicializáciu inteligetnej Ximea kamery tlaèidlo CONNECT,
    2. Spustenie a zastavenie snímkovania (akvizície) tlaèidlo START/STOP,
    3. Uloenie nastavení tlaèidlo SAVE a naèítanie poslednıch,
    4. Reinicializácia kamery tlaèidlom RE-CONNECT,			
    2. Nastavenie základnıch parametrov (Exposure, FPS, Gain) cez rozhranie API,
    3. Zistenie aktuálnej monej povolenej hodnoty a jej zobrazenie (Exposure, FPS, Gain),
    4. Sledovanie vıkonu (Performance): 
	a. Update - rıchlos zmeny nastavení cez API [ms] 
	b. Aquisition - rıchlos snímkovania [ms]
	c. Write - zápis na disk [ms]
	d. Total - celkovı èas [ms]
    5. Poèítadlo snímkov (Images)

 Záloka Images:

    1. Grid - zobrazenie vytvorenıch snímkov a automatickı refresh grid-u	
	    
![Alt text](/XIQCAMERA.jpg?raw=true "Optional Title")