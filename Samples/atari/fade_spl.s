;
;	Fade sample for consumption of fades created with 
;	Bitbendaz palette and raster toolkit:
;	https://github.com/MikaelStalvik/BBPaletteCalc
;
;

	section	text

init:
	pea			demo						;Run demo in supervisor
	move.w		#$26,-(sp)					;supexec()
	trap		#14					
	addq.l		#6,sp				

	clr.w		-(sp)						;pterm()
	trap		#1				


demo:		
	lea			save_screenadr,a0			;Save old screen address
	move.b		$ffff8201.w,(a0)+		
	move.b		$ffff8203.w,(a0)+		
	move.b		$ffff820d.w,(a0)+		

	movem.l		$ffff8240.w,d0-d7			;Save old palette
	movem.l		d0-d7,save_pal			

	move.l		#screen+256,d0				;Align new screen address
	clr.b		d0				

	lsr.w		#8,d0						;Set new screen address
	move.l		d0,$ffff8200.w			

	move.b		$ffff8260.w,save_res		;Save old resolution
	clr.b		$ffff8260.w					;Set low resolution

	move.w		#$2700,sr					;Stop all interrupts
	move.l		$70.w,save_vbl				;Save old VBL
	move.l		#vbl,$70.w					;Install our own VBL
	move.w		#$2300,sr					;Interrupts back on

	move.b		#$12,$fffffc02.w			;Kill mouse

    ;
    ; blit image
    ;
    movem.l     picture+2,d0-d7             ; Set palette
    movem.l     d0-d7,$ffff8240

    move.w      #2,-(a7)                    ; Get phybase
    trap        #14
    addq.l      #2,a7

    move.l      d0,a0                      ; a0 = screen memory
    move.l      #picture+34,a1             ; a1 = picture

    move.l      #8000-1,d0                  ; 8000 longwords = entire screen. Can be optimized with movem
.copy_loop:
    move.l      (a1)+,(a0)+
    dbf         d0,.copy_loop

;
; Demo main loop
;
.mainloop:	
	tst.w		vblcount					;Wait VBL
	beq.s		.mainloop			
	clr.w		vblcount			

    add.l       #1,fade_counter
    cmp.l       #96,fade_counter            ; 96 = fade speed, higher = slower
    bne         .no_change
    clr.l       fade_counter
    add.l       #1,fade_index
    cmp.l       #15,fade_index 
    ble.s       .no_change
    move.l      #15,fade_index              ; 15 = constrain to 16 fade indexes
.no_change:
    lea         fade_data,a0                ; fade palette in a2
    
    move.l      fade_index,d0               ; current palette in d0    
    lsl.l       #5,d0                       ; offset to correct palette index
    add.w       d0,a0                       ; add the offset to the address               
    movem.l     (a0),d0-d7
    movem.l     d0-d7,$ffff8240
	
    cmp.b		#$39,$fffffc02.w			;Space?
	bne.s		.mainloop			

;
; Deinit and exit
;
	move.w		#$2700,sr					;Stop all interrupts
	move.l		save_vbl,$70.w				;Restore old VBL
	move.w		#$2300,sr					;Interrupts back on

	move.b		save_res,$ffff8260.w		;Restore old resolution

	movem.l		save_pal,d0-d7				;Restore old palette
	movem.l		d0-d7,$ffff8240.w		

	lea			save_screenadr,a0			;Restore old screen address
	move.b		(a0)+,$ffff8201.w		
	move.b		(a0)+,$ffff8203.w		
	move.b		(a0)+,$ffff820d.w		

	move.b		#$8,$fffffc02.w				;Enable mouse

	rts


vbl:		
	addq.w		#1,vblcount
	rte


		section	data

picture:     incbin  "bblogo.pi1"       ; picture to show

; Paste the generated fade data here
fade_data:
	dc.w $000,$001,$112,$222,$22C,$32D,$33C,$33E,$C3D,$C3E,$DCD,$DCE,$EDE,$EDF,$FEF,$FFF
	dc.w $000,$008,$889,$999,$994,$A95,$AA4,$AA6,$4A5,$4A6,$545,$546,$656,$657,$767,$777
	dc.w $000,$008,$889,$999,$99B,$A9C,$AAB,$AAD,$BAC,$BAD,$CBC,$CBD,$DCD,$DCE,$EDE,$EEE
	dc.w $000,$008,$889,$999,$99B,$294,$22B,$225,$B24,$B25,$4B4,$4B5,$545,$546,$656,$666
	dc.w $000,$008,$881,$111,$113,$214,$223,$22C,$324,$32C,$434,$43C,$C4C,$C4D,$DCD,$DDD
	dc.w $000,$008,$881,$111,$113,$21B,$223,$224,$32B,$324,$B3B,$B34,$4B4,$4B5,$545,$555
	dc.w $000,$008,$881,$111,$11A,$913,$99A,$99B,$A93,$A9B,$3A3,$3AB,$B3B,$B3C,$CBC,$CCC
	dc.w $000,$008,$881,$111,$112,$91A,$992,$993,$29A,$293,$A2A,$A23,$3A3,$3A4,$434,$444
	dc.w $000,$000,$008,$888,$882,$18A,$112,$113,$21A,$213,$A2A,$A23,$3A3,$3AB,$B3B,$BBB
	dc.w $000,$000,$008,$888,$889,$182,$119,$11A,$912,$91A,$292,$29A,$A2A,$A23,$3A3,$333
	dc.w $000,$000,$008,$888,$889,$189,$119,$112,$919,$912,$999,$992,$292,$29A,$A2A,$AAA
	dc.w $000,$000,$008,$888,$881,$881,$881,$889,$181,$189,$111,$119,$919,$912,$292,$222
	dc.w $000,$000,$000,$000,$008,$801,$888,$881,$881,$881,$181,$181,$111,$119,$919,$999
	dc.w $000,$000,$000,$000,$008,$008,$008,$008,$808,$808,$888,$888,$888,$881,$181,$111
	dc.w $000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$008,$808,$888
	dc.w $000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000,$000



	section	bss

fade_counter        ds.l    0
fade_index:         ds.l    0

vblcount:			ds.w	1
screen:				ds.b	32000+256
save_pal:			ds.w	16
save_screenadr:		ds.l	1
save_vbl:			ds.l	1
save_res:			ds.b	1
		even

		end
