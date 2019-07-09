# XIQCAMERA - WPF aplikácia
Tento repozitár obsahuje vytvorenie WPF aplikácie na ovládanie Ximea kamery pomocou technológií:

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
	
![Alt text](/xiQCamera/screen1.jpg?raw=true "Optional Title")

 Záložka Images:

    1. Zobrazenie vytvorených snímkov v gride, 
	2. Sortovanie podľa času vytvorenia,
	3. Automatický refresh
	
![Alt text](/xiQCamera/screen2.jpg?raw=true "Optional Title")
	    
