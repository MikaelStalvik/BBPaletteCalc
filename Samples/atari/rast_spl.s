;
;	Raster sample for consumption of rasters created with 
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
	move.l		$68.w,save_hbl				;Save old HBL
	move.l		$134.w,save_ta				;Save old Timer A
	move.l		$120.w,save_tb				;Save old Timer B
	move.l		$114.w,save_tc				;Save old Timer C
	move.l		$110.w,save_td				;Save old Timer D
	move.l		$118.w,save_acia			;Save old ACIA
	move.l		#vbl,$70.w					;Install our own VBL
	move.l		#dummy,$68.w				;Install our own HBL (dummy)
	move.l		#dummy,$134.w				;Install our own Timer A (dummy)
	move.l		#timer_b,$120.w				;Install our own Timer B
	move.l		#dummy,$114.w				;Install our own Timer C (dummy)
	move.l		#dummy,$110.w				;Install our own Timer D (dummy)
	move.l		#dummy,$118.w				;Install our own ACIA (dummy)
	move.b		$fffffa09.w,save_intb		;Save MFP state for interrupt enable B
	move.b		$fffffa15.w,save_intb_mask	;Save MFP state for interrupt mask B
	clr.b		$fffffa07.w					;Interrupt enable A (Timer-A & B)
	clr.b		$fffffa13.w					;Interrupt mask A (Timer-A & B)
	clr.b		$fffffa09.w					;Interrupt enable B (Timer-C & D)
	clr.b		$fffffa15.w					;Interrupt mask B (Timer-C & D)
	move.w		#$2300,sr					;Interrupts back on

	move.b		#$12,$fffffc02.w			;Kill mouse

;
; Demo main loop
;
.mainloop:	
	tst.w		vblcount					;Wait VBL
	beq.s		.mainloop			
	clr.w		vblcount			

	cmp.b		#$39,$fffffc02.w			;Space?
	bne.s		.mainloop			

;
; Deinit and exit
;
	move.w		#$2700,sr					;Stop all interrupts
	move.l		save_vbl,$70.w				;Restore old VBL
	move.l		save_hbl,$68.w				;Restore old HBL
	move.l		save_ta,$134.w				;Restore old Timer A
	move.l		save_tb,$120.w				;Restore old Timer B
	move.l		save_tc,$114.w				;Restore old Timer C
	move.l		save_td,$110.w				;Restore old Timer D
	move.l		save_acia,$118.w			;Restore old ACIA
	move.b		save_intb,$fffffa09.w		;Restore MFP state for interrupt enable B
	move.b		save_intb_mask,$fffffa15.w	;Restore MFP state for interrupt mask B
	clr.b		$fffffa1b.w					;Timer B control (Stop)
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
	clr.w		raster_ofs

	;Start up Timer B each VBL
	move.w		#$2700,sr				;Stop all interrupts
	clr.b		$fffffa1b.w				;Timer B control (stop)
	bset		#0,$fffffa07.w			;Interrupt enable A (Timer B)
	bset		#0,$fffffa13.w			;Interrupt mask A (Timer B)
	move.b		#1,$fffffa21.w			;Timer B data (number of scanlines to next interrupt)
	bclr		#3,$fffffa17.w			;Automatic end of interrupt
	move.b		#8,$fffffa1b.w			;Timer B control (event mode (HBL))
	move.w		#$2300,sr				;Interrupts back on

	rte

timer_b:	
	move.l		a0,-(sp)				;Save A0
	lea			raster_data,a0			;Raster list
	add.w		raster_ofs,a0			;Raster list offset
	move.w		(a0),$ffff8240.w		;Set colour
	addq.w		#2,raster_ofs			;Next raster offset
	move.l		(sp)+,a0				;Restore A0

	rte

dummy:		
	rte

		section	data

; Paste the generated raster data here
; 200 items
raster_data:
	dc.w  $000,$000,$000,$000,$000,$000,$808,$808,$808
	dc.w  $808,$808,$108,$101,$101,$101,$101,$901,$901
	dc.w  $909,$909,$909,$209,$209,$209,$202,$202,$A02
	dc.w  $A02,$A02,$A02,$A0A,$30A,$30A,$30A,$30A,$30A
	dc.w  $B03,$B03,$B03,$B03,$B03,$B03,$40B,$40B,$40B
	dc.w  $40B,$40B,$C0B,$C04,$C04,$C04,$C04,$504,$504
	dc.w  $50C,$50C,$50C,$D0C,$D0C,$D0C,$D05,$D05,$605
	dc.w  $605,$605,$605,$60D,$E0D,$E0D,$E0D,$E0D,$E0D
	dc.w  $706,$706,$706,$706,$706,$F0E,$F0E,$70E,$70E
	dc.w  $70E,$70E,$70E,$E0E,$E8E,$E8E,$E8E,$E8E,$68E
	dc.w  $68E,$61E,$61E,$61E,$D1E,$D1E,$D1E,$D9E,$D9E
	dc.w  $D9E,$59E,$59E,$59E,$59E,$52E,$C2E,$C2E,$C2E
	dc.w  $C2E,$C2E,$4A7,$4A7,$4A7,$4A7,$4A7,$BA7,$B37
	dc.w  $B37,$B37,$B37,$B37,$337,$337,$3B7,$3B7,$3B7
	dc.w  $AB7,$AB7,$AB7,$A47,$A47,$247,$247,$247,$247
	dc.w  $2C7,$9C7,$9C7,$9C7,$9C7,$9C7,$95F,$95F,$95F
	dc.w  $95F,$95F,$95F,$95F,$95F,$95F,$95F,$95F,$25F
	dc.w  $25F,$25F,$25F,$2DF,$2DF,$2DF,$2DF,$2DF,$ADF
	dc.w  $ADF,$ADF,$ADF,$ADF,$ADF,$ADF,$ADF,$ADF,$36F
	dc.w  $36F,$36F,$36F,$36F,$36F,$36F,$36F,$36F,$36F
	dc.w  $B6F,$B6F,$B6F,$B6F,$BEF,$BEF,$BEF,$BEF,$BEF
	dc.w  $4EF,$4EF,$4EF,$4EF,$4EF,$4EF,$4EF,$4EF,$4EF
	dc.w  $C7F,$C7F

	section	bss

raster_ofs:			ds.w	1
vblcount:			ds.w	1
screen:				ds.b	32000+256
save_pal:			ds.w	16
save_screenadr:		ds.l	1
save_vbl:			ds.l	1
save_hbl:			ds.l	1
save_ta:			ds.l	1
save_tb:			ds.l	1
save_tc:			ds.l	1
save_td:			ds.l	1
save_acia:			ds.l	1
save_intb:			ds.b	1
save_intb_mask:		ds.b	1
save_res:			ds.b	1
		even

		end
