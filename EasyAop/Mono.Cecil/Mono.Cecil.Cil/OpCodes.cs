namespace Mono.Cecil.Cil
{
	public static class OpCodes
	{
		internal static readonly OpCode[] OneByteOpCode = new OpCode[225];

		internal static readonly OpCode[] TwoBytesOpCode = new OpCode[31];

		public static readonly OpCode Nop = new OpCode(83886335, 318768389);

		public static readonly OpCode Break = new OpCode(16843263, 318768389);

		public static readonly OpCode Ldarg_0 = new OpCode(84017919, 335545601);

		public static readonly OpCode Ldarg_1 = new OpCode(84083711, 335545601);

		public static readonly OpCode Ldarg_2 = new OpCode(84149503, 335545601);

		public static readonly OpCode Ldarg_3 = new OpCode(84215295, 335545601);

		public static readonly OpCode Ldloc_0 = new OpCode(84281087, 335545601);

		public static readonly OpCode Ldloc_1 = new OpCode(84346879, 335545601);

		public static readonly OpCode Ldloc_2 = new OpCode(84412671, 335545601);

		public static readonly OpCode Ldloc_3 = new OpCode(84478463, 335545601);

		public static readonly OpCode Stloc_0 = new OpCode(84544255, 318833921);

		public static readonly OpCode Stloc_1 = new OpCode(84610047, 318833921);

		public static readonly OpCode Stloc_2 = new OpCode(84675839, 318833921);

		public static readonly OpCode Stloc_3 = new OpCode(84741631, 318833921);

		public static readonly OpCode Ldarg_S = new OpCode(84807423, 335549185);

		public static readonly OpCode Ldarga_S = new OpCode(84873215, 369103617);

		public static readonly OpCode Starg_S = new OpCode(84939007, 318837505);

		public static readonly OpCode Ldloc_S = new OpCode(85004799, 335548929);

		public static readonly OpCode Ldloca_S = new OpCode(85070591, 369103361);

		public static readonly OpCode Stloc_S = new OpCode(85136383, 318837249);

		public static readonly OpCode Ldnull = new OpCode(85202175, 436208901);

		public static readonly OpCode Ldc_I4_M1 = new OpCode(85267967, 369100033);

		public static readonly OpCode Ldc_I4_0 = new OpCode(85333759, 369100033);

		public static readonly OpCode Ldc_I4_1 = new OpCode(85399551, 369100033);

		public static readonly OpCode Ldc_I4_2 = new OpCode(85465343, 369100033);

		public static readonly OpCode Ldc_I4_3 = new OpCode(85531135, 369100033);

		public static readonly OpCode Ldc_I4_4 = new OpCode(85596927, 369100033);

		public static readonly OpCode Ldc_I4_5 = new OpCode(85662719, 369100033);

		public static readonly OpCode Ldc_I4_6 = new OpCode(85728511, 369100033);

		public static readonly OpCode Ldc_I4_7 = new OpCode(85794303, 369100033);

		public static readonly OpCode Ldc_I4_8 = new OpCode(85860095, 369100033);

		public static readonly OpCode Ldc_I4_S = new OpCode(85925887, 369102849);

		public static readonly OpCode Ldc_I4 = new OpCode(85991679, 369099269);

		public static readonly OpCode Ldc_I8 = new OpCode(86057471, 385876741);

		public static readonly OpCode Ldc_R4 = new OpCode(86123263, 402657541);

		public static readonly OpCode Ldc_R8 = new OpCode(86189055, 419432197);

		public static readonly OpCode Dup = new OpCode(86255103, 352388357);

		public static readonly OpCode Pop = new OpCode(86320895, 318833925);

		public static readonly OpCode Jmp = new OpCode(36055039, 318768133);

		public static readonly OpCode Call = new OpCode(36120831, 471532549);

		public static readonly OpCode Calli = new OpCode(36186623, 471533573);

		public static readonly OpCode Ret = new OpCode(120138495, 320537861);

		public static readonly OpCode Br_S = new OpCode(2763775, 318770945);

		public static readonly OpCode Brfalse_S = new OpCode(53161215, 318967553);

		public static readonly OpCode Brtrue_S = new OpCode(53227007, 318967553);

		public static readonly OpCode Beq_S = new OpCode(53292799, 318902017);

		public static readonly OpCode Bge_S = new OpCode(53358591, 318902017);

		public static readonly OpCode Bgt_S = new OpCode(53424383, 318902017);

		public static readonly OpCode Ble_S = new OpCode(53490175, 318902017);

		public static readonly OpCode Blt_S = new OpCode(53555967, 318902017);

		public static readonly OpCode Bne_Un_S = new OpCode(53621759, 318902017);

		public static readonly OpCode Bge_Un_S = new OpCode(53687551, 318902017);

		public static readonly OpCode Bgt_Un_S = new OpCode(53753343, 318902017);

		public static readonly OpCode Ble_Un_S = new OpCode(53819135, 318902017);

		public static readonly OpCode Blt_Un_S = new OpCode(53884927, 318902017);

		public static readonly OpCode Br = new OpCode(3619071, 318767109);

		public static readonly OpCode Brfalse = new OpCode(54016511, 318963717);

		public static readonly OpCode Brtrue = new OpCode(54082303, 318963717);

		public static readonly OpCode Beq = new OpCode(54148095, 318898177);

		public static readonly OpCode Bge = new OpCode(54213887, 318898177);

		public static readonly OpCode Bgt = new OpCode(54279679, 318898177);

		public static readonly OpCode Ble = new OpCode(54345471, 318898177);

		public static readonly OpCode Blt = new OpCode(54411263, 318898177);

		public static readonly OpCode Bne_Un = new OpCode(54477055, 318898177);

		public static readonly OpCode Bge_Un = new OpCode(54542847, 318898177);

		public static readonly OpCode Bgt_Un = new OpCode(54608639, 318898177);

		public static readonly OpCode Ble_Un = new OpCode(54674431, 318898177);

		public static readonly OpCode Blt_Un = new OpCode(54740223, 318898177);

		public static readonly OpCode Switch = new OpCode(54806015, 318966277);

		public static readonly OpCode Ldind_I1 = new OpCode(88426239, 369296645);

		public static readonly OpCode Ldind_U1 = new OpCode(88492031, 369296645);

		public static readonly OpCode Ldind_I2 = new OpCode(88557823, 369296645);

		public static readonly OpCode Ldind_U2 = new OpCode(88623615, 369296645);

		public static readonly OpCode Ldind_I4 = new OpCode(88689407, 369296645);

		public static readonly OpCode Ldind_U4 = new OpCode(88755199, 369296645);

		public static readonly OpCode Ldind_I8 = new OpCode(88820991, 386073861);

		public static readonly OpCode Ldind_I = new OpCode(88886783, 369296645);

		public static readonly OpCode Ldind_R4 = new OpCode(88952575, 402851077);

		public static readonly OpCode Ldind_R8 = new OpCode(89018367, 419628293);

		public static readonly OpCode Ldind_Ref = new OpCode(89084159, 436405509);

		public static readonly OpCode Stind_Ref = new OpCode(89149951, 319096069);

		public static readonly OpCode Stind_I1 = new OpCode(89215743, 319096069);

		public static readonly OpCode Stind_I2 = new OpCode(89281535, 319096069);

		public static readonly OpCode Stind_I4 = new OpCode(89347327, 319096069);

		public static readonly OpCode Stind_I8 = new OpCode(89413119, 319161605);

		public static readonly OpCode Stind_R4 = new OpCode(89478911, 319292677);

		public static readonly OpCode Stind_R8 = new OpCode(89544703, 319358213);

		public static readonly OpCode Add = new OpCode(89610495, 335676677);

		public static readonly OpCode Sub = new OpCode(89676287, 335676677);

		public static readonly OpCode Mul = new OpCode(89742079, 335676677);

		public static readonly OpCode Div = new OpCode(89807871, 335676677);

		public static readonly OpCode Div_Un = new OpCode(89873663, 335676677);

		public static readonly OpCode Rem = new OpCode(89939455, 335676677);

		public static readonly OpCode Rem_Un = new OpCode(90005247, 335676677);

		public static readonly OpCode And = new OpCode(90071039, 335676677);

		public static readonly OpCode Or = new OpCode(90136831, 335676677);

		public static readonly OpCode Xor = new OpCode(90202623, 335676677);

		public static readonly OpCode Shl = new OpCode(90268415, 335676677);

		public static readonly OpCode Shr = new OpCode(90334207, 335676677);

		public static readonly OpCode Shr_Un = new OpCode(90399999, 335676677);

		public static readonly OpCode Neg = new OpCode(90465791, 335611141);

		public static readonly OpCode Not = new OpCode(90531583, 335611141);

		public static readonly OpCode Conv_I1 = new OpCode(90597375, 369165573);

		public static readonly OpCode Conv_I2 = new OpCode(90663167, 369165573);

		public static readonly OpCode Conv_I4 = new OpCode(90728959, 369165573);

		public static readonly OpCode Conv_I8 = new OpCode(90794751, 385942789);

		public static readonly OpCode Conv_R4 = new OpCode(90860543, 402720005);

		public static readonly OpCode Conv_R8 = new OpCode(90926335, 419497221);

		public static readonly OpCode Conv_U4 = new OpCode(90992127, 369165573);

		public static readonly OpCode Conv_U8 = new OpCode(91057919, 385942789);

		public static readonly OpCode Callvirt = new OpCode(40792063, 471532547);

		public static readonly OpCode Cpobj = new OpCode(91189503, 319097859);

		public static readonly OpCode Ldobj = new OpCode(91255295, 335744003);

		public static readonly OpCode Ldstr = new OpCode(91321087, 436209923);

		public static readonly OpCode Newobj = new OpCode(41055231, 437978115);

		public static readonly OpCode Castclass = new OpCode(91452671, 436866051);

		public static readonly OpCode Isinst = new OpCode(91518463, 369757187);

		public static readonly OpCode Conv_R_Un = new OpCode(91584255, 419497221);

		public static readonly OpCode Unbox = new OpCode(91650559, 369757189);

		public static readonly OpCode Throw = new OpCode(142047999, 319423747);

		public static readonly OpCode Ldfld = new OpCode(91782143, 336199939);

		public static readonly OpCode Ldflda = new OpCode(91847935, 369754371);

		public static readonly OpCode Stfld = new OpCode(91913727, 319488259);

		public static readonly OpCode Ldsfld = new OpCode(91979519, 335544579);

		public static readonly OpCode Ldsflda = new OpCode(92045311, 369099011);

		public static readonly OpCode Stsfld = new OpCode(92111103, 318832899);

		public static readonly OpCode Stobj = new OpCode(92176895, 319032323);

		public static readonly OpCode Conv_Ovf_I1_Un = new OpCode(92242687, 369165573);

		public static readonly OpCode Conv_Ovf_I2_Un = new OpCode(92308479, 369165573);

		public static readonly OpCode Conv_Ovf_I4_Un = new OpCode(92374271, 369165573);

		public static readonly OpCode Conv_Ovf_I8_Un = new OpCode(92440063, 385942789);

		public static readonly OpCode Conv_Ovf_U1_Un = new OpCode(92505855, 369165573);

		public static readonly OpCode Conv_Ovf_U2_Un = new OpCode(92571647, 369165573);

		public static readonly OpCode Conv_Ovf_U4_Un = new OpCode(92637439, 369165573);

		public static readonly OpCode Conv_Ovf_U8_Un = new OpCode(92703231, 385942789);

		public static readonly OpCode Conv_Ovf_I_Un = new OpCode(92769023, 369165573);

		public static readonly OpCode Conv_Ovf_U_Un = new OpCode(92834815, 369165573);

		public static readonly OpCode Box = new OpCode(92900607, 436276229);

		public static readonly OpCode Newarr = new OpCode(92966399, 436407299);

		public static readonly OpCode Ldlen = new OpCode(93032191, 369755395);

		public static readonly OpCode Ldelema = new OpCode(93097983, 369888259);

		public static readonly OpCode Ldelem_I1 = new OpCode(93163775, 369886467);

		public static readonly OpCode Ldelem_U1 = new OpCode(93229567, 369886467);

		public static readonly OpCode Ldelem_I2 = new OpCode(93295359, 369886467);

		public static readonly OpCode Ldelem_U2 = new OpCode(93361151, 369886467);

		public static readonly OpCode Ldelem_I4 = new OpCode(93426943, 369886467);

		public static readonly OpCode Ldelem_U4 = new OpCode(93492735, 369886467);

		public static readonly OpCode Ldelem_I8 = new OpCode(93558527, 386663683);

		public static readonly OpCode Ldelem_I = new OpCode(93624319, 369886467);

		public static readonly OpCode Ldelem_R4 = new OpCode(93690111, 403440899);

		public static readonly OpCode Ldelem_R8 = new OpCode(93755903, 420218115);

		public static readonly OpCode Ldelem_Ref = new OpCode(93821695, 436995331);

		public static readonly OpCode Stelem_I = new OpCode(93887487, 319620355);

		public static readonly OpCode Stelem_I1 = new OpCode(93953279, 319620355);

		public static readonly OpCode Stelem_I2 = new OpCode(94019071, 319620355);

		public static readonly OpCode Stelem_I4 = new OpCode(94084863, 319620355);

		public static readonly OpCode Stelem_I8 = new OpCode(94150655, 319685891);

		public static readonly OpCode Stelem_R4 = new OpCode(94216447, 319751427);

		public static readonly OpCode Stelem_R8 = new OpCode(94282239, 319816963);

		public static readonly OpCode Stelem_Ref = new OpCode(94348031, 319882499);

		public static readonly OpCode Ldelem_Any = new OpCode(94413823, 336333827);

		public static readonly OpCode Stelem_Any = new OpCode(94479615, 319884291);

		public static readonly OpCode Unbox_Any = new OpCode(94545407, 336202755);

		public static readonly OpCode Conv_Ovf_I1 = new OpCode(94614527, 369165573);

		public static readonly OpCode Conv_Ovf_U1 = new OpCode(94680319, 369165573);

		public static readonly OpCode Conv_Ovf_I2 = new OpCode(94746111, 369165573);

		public static readonly OpCode Conv_Ovf_U2 = new OpCode(94811903, 369165573);

		public static readonly OpCode Conv_Ovf_I4 = new OpCode(94877695, 369165573);

		public static readonly OpCode Conv_Ovf_U4 = new OpCode(94943487, 369165573);

		public static readonly OpCode Conv_Ovf_I8 = new OpCode(95009279, 385942789);

		public static readonly OpCode Conv_Ovf_U8 = new OpCode(95075071, 385942789);

		public static readonly OpCode Refanyval = new OpCode(95142655, 369167365);

		public static readonly OpCode Ckfinite = new OpCode(95208447, 419497221);

		public static readonly OpCode Mkrefany = new OpCode(95274751, 335744005);

		public static readonly OpCode Ldtoken = new OpCode(95342847, 369101573);

		public static readonly OpCode Conv_U2 = new OpCode(95408639, 369165573);

		public static readonly OpCode Conv_U1 = new OpCode(95474431, 369165573);

		public static readonly OpCode Conv_I = new OpCode(95540223, 369165573);

		public static readonly OpCode Conv_Ovf_I = new OpCode(95606015, 369165573);

		public static readonly OpCode Conv_Ovf_U = new OpCode(95671807, 369165573);

		public static readonly OpCode Add_Ovf = new OpCode(95737599, 335676677);

		public static readonly OpCode Add_Ovf_Un = new OpCode(95803391, 335676677);

		public static readonly OpCode Mul_Ovf = new OpCode(95869183, 335676677);

		public static readonly OpCode Mul_Ovf_Un = new OpCode(95934975, 335676677);

		public static readonly OpCode Sub_Ovf = new OpCode(96000767, 335676677);

		public static readonly OpCode Sub_Ovf_Un = new OpCode(96066559, 335676677);

		public static readonly OpCode Endfinally = new OpCode(129686783, 318768389);

		public static readonly OpCode Leave = new OpCode(12312063, 319946757);

		public static readonly OpCode Leave_S = new OpCode(12377855, 319950593);

		public static readonly OpCode Stind_I = new OpCode(96329727, 319096069);

		public static readonly OpCode Conv_U = new OpCode(96395519, 369165573);

		public static readonly OpCode Arglist = new OpCode(96403710, 369100037);

		public static readonly OpCode Ceq = new OpCode(96469502, 369231109);

		public static readonly OpCode Cgt = new OpCode(96535294, 369231109);

		public static readonly OpCode Cgt_Un = new OpCode(96601086, 369231109);

		public static readonly OpCode Clt = new OpCode(96666878, 369231109);

		public static readonly OpCode Clt_Un = new OpCode(96732670, 369231109);

		public static readonly OpCode Ldftn = new OpCode(96798462, 369099781);

		public static readonly OpCode Ldvirtftn = new OpCode(96864254, 369755141);

		public static readonly OpCode Ldarg = new OpCode(96930302, 335547909);

		public static readonly OpCode Ldarga = new OpCode(96996094, 369102341);

		public static readonly OpCode Starg = new OpCode(97061886, 318836229);

		public static readonly OpCode Ldloc = new OpCode(97127678, 335547653);

		public static readonly OpCode Ldloca = new OpCode(97193470, 369102085);

		public static readonly OpCode Stloc = new OpCode(97259262, 318835973);

		public static readonly OpCode Localloc = new OpCode(97325054, 369296645);

		public static readonly OpCode Endfilter = new OpCode(130945534, 318964997);

		public static readonly OpCode Unaligned = new OpCode(80679678, 318771204);

		public static readonly OpCode Volatile = new OpCode(80745470, 318768388);

		public static readonly OpCode Tail = new OpCode(80811262, 318768388);

		public static readonly OpCode Initobj = new OpCode(97654270, 318966787);

		public static readonly OpCode Constrained = new OpCode(97720062, 318770180);

		public static readonly OpCode Cpblk = new OpCode(97785854, 319227141);

		public static readonly OpCode Initblk = new OpCode(97851646, 319227141);

		public static readonly OpCode No = new OpCode(97917438, 318771204);

		public static readonly OpCode Rethrow = new OpCode(148314878, 318768387);

		public static readonly OpCode Sizeof = new OpCode(98049278, 369101829);

		public static readonly OpCode Refanytype = new OpCode(98115070, 369165573);

		public static readonly OpCode Readonly = new OpCode(98180862, 318768388);
	}
}
