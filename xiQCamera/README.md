# XIQCAMERA - WPF aplikácia
Tento repozitár obsahuje vytvorenie WPF aplikácie na ovládanie Ximea kamery za pomoci technológií:

    a. C#,
    b. WPF
    c. xiAPI.dll

### XIQCAMERA aplikácia umožňuje:

 Záložka Camera:

    1. Inicializáciu inteligetnej Ximea kamery tlačidlo CONNECT,
    2. Spustenie a zastavenie snímkovania (akvizície) tlačidlo START/STOP,
    3. Uloženie nastavení tlačidlo SAVE a načítanie posledných,
    4. Reinicializácia kamery tlačidlom RE-CONNECT,			
    2. Nastavenie základných parametrov (Exposure, FPS, Gain) cez rozhranie API,
    3. Zistenie aktuálnej možnej povolenej hodnoty a jej zobrazenie (Exposure, FPS, Gain),
    4. Sledovanie výkonu (Performance): 
	a. Update - rýchlosť zmeny nastavení cez API [ms] 
	b. Aquisition - rýchlosť snímkovania [ms]
	c. Write - zápis na disk [ms]
	d. Total - celkový čas [ms]
    5. Počítadlo snímkov (Images)

 Záložka Images:

    1. Grid - zobrazenie vytvorených snímkov a automatický refresh grid-u	
	    
![Alt text](/xiQCamera/XIQCAMERA.jpg?raw=true "Optional Title")
