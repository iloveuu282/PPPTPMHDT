const {chromium}=require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const definitions=JSON.parse(fs.readFileSync(path.resolve(__dirname,'../docs/nghiep-vu/06_BoQuyChe_ChotDemo.json'),'utf8').replace(/^\uFEFF/,''));
const time=definitions.find(d=>d.code==='QC-THOIGIAN-LAMVIEC');
const start=time.clauses.find(c=>c.topic==='WorkStart').body, end=time.clauses.find(c=>c.topic==='WorkEnd').body;
(async()=>{
 const base=process.env.UI_BASE_URL||'http://localhost:5081',mutate=process.env.UI_ADMIN_MUTATIONS==='1';
 const adminPassword=process.env.UI_ADMIN_PASSWORD || process.env.DemoAdmin__Password;
 if(!adminPassword)throw Error('Cần đặt UI_ADMIN_PASSWORD hoặc DemoAdmin__Password để kiểm tra đăng nhập.');
 if(mutate && !base.includes(':5081'))throw Error('Chỉ thử thay quy chế trên cổng demo 5081.');
 const out=path.resolve(__dirname,'../output/final-ui'+(base.includes(':5080')?'-main':''));fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({headless:true,executablePath:process.env.UI_BROWSER||undefined});
 const context=await browser.newContext({viewport:{width:1440,height:1000}}),page=await context.newPage(),checks=[],errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 async function check(name,run){await run();checks.push(name);console.log('PASS '+name);}
 async function ask(q,date){await page.goto(base);await page.locator('#Question').fill(q);if(date){await page.locator('.date-options summary').click();await page.locator('#ApplicableDate').fill(date);}await page.locator('#ask-button').click();await page.locator('#ket-qua').waitFor();return {url:page.url(),text:await page.locator('.response').innerText(),sources:await page.locator('.source-link').evaluateAll(es=>es.map(e=>e.getAttribute('href')))};}
 try{
 await page.goto(base);
 await check('CSS và JavaScript tải thực tế',async()=>{assert.equal(await page.locator('.workspace').evaluate(e=>getComputedStyle(e).display),'grid');assert.equal((await context.request.get(base+await page.locator('link[rel="stylesheet"]').getAttribute('href'))).status(),200);});
 await check('Ba bộ quy chế hoàn chỉnh và giờ mới',async()=>{await page.goto(base+'/quy-che');assert.equal(await page.locator('.policy-catalog details').count(),3);for(const el of await page.locator('.policy-catalog details').all())await el.locator('summary').click();const text=await page.locator('.policy-catalog').innerText();for(const word of [start,end,'thứ Hai','12 giờ','7 giờ','3 ngày','8 ngày'])assert.ok(text.includes(word),word);await page.screenshot({path:path.join(out,'01-quy-che.png'),fullPage:true});});
 let saved;
 const cases=[
 ['Giờ bắt đầu vào làm?',[start],1],['Giờ tan làm?',[end],1],
 ['Làm ơn cho biết giờ tan làm?',[end],1],['Không hỏi giờ vào làm, tôi hỏi giờ tan làm?',[end],1],
 ['Mỗi tuần làm việc bao nhiêu ngày?',['thứ Hai','thứ Sáu'],1],['Nghỉ trưa bao lâu?',['12 giờ','13 giờ'],1],
 ['Được xin nghỉ phép bao nhiêu ngày?',['3 ngày','8 ngày','đồng thời'],3],
 ['Xin nghỉ báo trước bao lâu?',['1 ngày làm việc','5 ngày làm việc'],2],
 ['Nghỉ từ 2 ngày trở lên cần báo trước bao lâu?',['5 ngày làm việc'],1],
 ['Nghỉ hơn 1 ngày làm việc cần báo trước mấy ngày?',['5 ngày làm việc'],1],
 ['Xin nghỉ có cần quản lý duyệt không?',['quản lý trực tiếp'],1],
 ['Nghỉ 4 ngày trong một tuần ai duyệt?',['3 ngày','quản lý trực tiếp','trưởng bộ phận'],3],
 ['Được nghỉ tối đa bao nhiêu ngày mỗi năm?',['Chưa đủ căn cứ','theo năm'],0],
 ['Nghỉ 999999999999999999999999999999999999 ngày báo trước bao lâu?',['Vui lòng'],0],
 ['Nghỉ bao lâu?',['Bạn hỏi'],0],['Giờ vào làm theo quy chế cũ?',['ngày hoặc tháng/năm'],0],
 ['Giờ bắt đầu vào làm tháng 5/2026?',['8 giờ sáng'],1],['Giờ tan làm tháng 8/2026?',['16 giờ (4 giờ chiều)'],1]
 ];
 for(const[q,words,count]of cases)await check(q,async()=>{const r=await ask(q);for(const w of words)assert.ok(r.text.includes(w),r.text);assert.equal(r.sources.length,count);if(q==='Xin nghỉ báo trước bao lâu?'){saved=r;await page.screenshot({path:path.join(out,'02-tra-loi.png'),fullPage:true});}});
 await check('Một năm sau vẫn áp dụng bản không hết hạn',async()=>assert.ok((await ask('Giờ bắt đầu vào làm?','2027-11-11')).text.includes(start)));
 await check('Lịch sử và nguồn giữ đúng phiên bản',async()=>{await page.goto(saved.url);assert.deepEqual(await page.locator('.source-link').evaluateAll(es=>es.map(e=>e.getAttribute('href'))),saved.sources);for(const src of saved.sources)assert.equal((await context.request.get(base+src)).status(),200);});
 await check('Trình duyệt khác không mở lịch sử và nguồn riêng',async()=>{const other=await browser.newContext();assert.equal((await other.request.get(saved.url)).status(),404);assert.equal((await other.request.get(base+saved.sources[0])).status(),404);await other.close();});
 await check('Khách và POST không có chống giả mạo bị chặn',async()=>{assert.equal((await context.request.get(base+'/Admin',{maxRedirects:0})).status(),302);assert.equal((await context.request.post(base+'/',{form:{Question:'Giờ vào làm?'}})).status(),400);});
 await check('Không thực thi HTML trong câu hỏi',async()=>{await ask('<script>window.FINAL_XSS=1</script> Giờ tan làm?');assert.equal(await page.evaluate(()=>window.FINAL_XSS),undefined);assert.equal(await page.locator('.asked-question script').count(),0);});
 await check('Không tràn ngang ở 320, 390, 820 và 1440px',async()=>{await page.goto(saved.url);for(const width of[320,390,820,1440]){await page.setViewportSize({width,height:1000});assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));}await page.setViewportSize({width:390,height:844});await page.screenshot({path:path.join(out,'03-dien-thoai.png'),fullPage:true});await page.setViewportSize({width:1440,height:1000});});
 await check('Đăng nhập quản trị',async()=>{await page.goto(base+'/Account/Login');await page.locator('#Username').fill('admin');await page.locator('#Password').fill(adminPassword);await page.getByRole('button',{name:'Đăng nhập',exact:true}).click();await page.waitForURL('**/Admin**');assert.equal(await page.locator('.document-card').count(),3);});
 if(mutate){
   let oldOffice;
   await check('Lưu nguồn trước khi thử thay quy chế',async()=>{oldOffice=await ask('Vệ sinh chỗ làm thế nào?');assert.equal(oldOffice.sources.length,1);});
   async function draft(label){await page.goto(base+'/Admin/Create?documentId=103');assert.ok(await page.locator('#CopyFrom').inputValue());await page.locator('#Label').fill(label);await page.getByRole('button',{name:'Tạo bản nháp',exact:true}).click();await page.waitForURL('**/Admin/Edit/**');}
   async function field(topic){for(const f of await page.locator('fieldset').all())if(await f.locator('select').inputValue()===topic)return f;throw Error('Missing '+topic);}
   await check('Sao chép nháp và cảnh báo chủ đề bị bỏ',async()=>{await draft('ui-partial');await (await field('Visitors')).locator('input[type="checkbox"][name$=".Remove"]').check();await page.getByRole('button',{name:'Lưu bản nháp',exact:true}).click();await page.locator('#AcknowledgeMissingTopics').waitFor();assert.match(await page.locator('.notice[role="alert"]').innerText(),/Tiếp khách/);});
   await check('Máy chủ chặn duyệt thiếu chủ đề khi chưa xác nhận',async()=>{await page.locator('#ReplaceCurrent').check();await page.locator('form[action*="handler=Approve"] input[type="checkbox"][required]').check();await page.getByRole('button',{name:'Duyệt phiên bản đã lưu'}).click();assert.match(await page.locator('.validation-summary-errors').innerText(),/thiếu các chủ đề/);});
   await check('Lưu nháp, duyệt thay thế, đáp án đổi theo SQL',async()=>{await draft('ui-complete');const f=await field('OfficeCleanliness');const body=f.locator('textarea');await body.fill((await body.inputValue())+' Dọn bàn trước khi ra về.');await page.getByRole('button',{name:'Lưu bản nháp',exact:true}).click();await page.locator('#ReplaceCurrent').check();await page.locator('form[action*="handler=Approve"] input[type="checkbox"][required]').check();await page.getByRole('button',{name:'Duyệt phiên bản đã lưu'}).click();assert.ok((await ask('Vệ sinh chỗ làm thế nào?')).text.includes('Dọn bàn trước khi ra về.'));});
   await check('Nguồn cũ không bị đổi sau khi duyệt thay thế',async()=>{await page.goto(oldOffice.url);assert.equal(await page.locator('.response').innerText(),oldOffice.text);assert.equal((await context.request.get(base+oldOffice.sources[0])).status(),200);});
 }
 await check('Đăng xuất và không còn trang học hệ thống',async()=>{await page.goto(base+'/Admin');await page.getByRole('button',{name:'Đăng xuất',exact:true}).click();assert.equal((await context.request.get(base+'/Admin',{maxRedirects:0})).status(),302);for(const p of['/Admin/Learn','/Admin/Explain'])assert.equal((await context.request.get(base+p)).status(),404);assert.deepEqual(errors,[]);});
 await context.storageState({path:path.join(out,'browser-state.json')});
 fs.writeFileSync(path.join(out,'results.json'),JSON.stringify({base,AdminMutations:mutate,Passed:checks.length,Checks:checks,History:saved},null,2));
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});
