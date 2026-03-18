const fetch = require('node-fetch');

// clientKey：从账户获取
const clientKey = "xxxxxxxxx";
// 目标参数：
const websiteKey = "6Le-wvkSAAAAAPBMRTvw0Q4Muexq9bi0DJwx_mJ-";
const websiteURL = "https://www.google.com/recaptcha/api2/demo";
// 验证码类型：
const taskType = "NoCaptchaTaskProxyless";

async function createTask() {
  try {
    const url = "https://api.yescaptcha.com/createTask";
    const data = {
      clientKey: clientKey,
      task: {
        websiteURL: websiteURL,
        websiteKey: websiteKey,
        type: taskType
      }
    };

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data),
      agent: false,
      rejectUnauthorized: false
    });

    const result = await response.json();
    const taskId = result.taskId;
    if (taskId) {
      return taskId;
    } else {
      console.log(result);
    }
  } catch (error) {
    console.log(error);
  }
}

async function getResponse(taskID) {
  let times = 0;
  while (times < 120) {
    try {
      const url = "https://api.yescaptcha.com/getTaskResult";
      const data = {
        clientKey: clientKey,
        taskId: taskID
      };

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(data),
        agent: false,
        rejectUnauthorized: false
      });

      const result = await response.json();
      const solution = result.solution;
      if (solution) {
        const response = solution.gRecaptchaResponse;
        if (response) {
          return response;
        }
      } else {
        console.log(result);
      }
    } catch (error) {
      console.log(error);
    }
    times += 3;
    await new Promise(resolve => setTimeout(resolve, 3000)); // 等待3秒钟
  }
}

async function verifyWebsite(response) {
  const url = "https://www.google.com/recaptcha/api2/demo";
  const data = { "g-recaptcha-response": response };

  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams(data),
    agent: false,
    rejectUnauthorized: false
  });

  if (response.status === 200) {
    const result = await response.text();
    return result;
  }
}

(async () => {
  const taskId = await createTask();
  console.log('创建任务:', taskId);
  if (taskId) {
    const response = await getResponse(taskId);
    console.log('识别结果:', response);
    const result = await verifyWebsite(response);
    console.log('验证结果:', result);
  }
})();
