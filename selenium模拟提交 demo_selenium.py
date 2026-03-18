import time
import requests
from selenium import webdriver

""" 
YESCAPTCHA验证码DEMO selenium版本
目标网站 https://www.google.com/recaptcha/api2/demo
这是谷歌官方的演示
谷歌官方是reCaptcha V2
这里只是演示简单的处理
不同的网站需要针对性的提交
参考这个思路即可
不要生搬硬套
"""

# clientKey：在个人中心获取
clientKey = "cc9c18d3e263515c2c072b36a7125eecc078618f3"
# 目标参数：
websiteKey = "6Le-wvkSAAAAAPBMRTvw0Q4Muexq9bi0DJwx_mJ-"
# 目标参数：
websiteURL = "https://www.google.com/recaptcha/api2/demo"
# 验证码类型：
task_type = "NoCaptchaTaskProxyless"



def create_task() -> str:
    """ 
    第一步，创建验证码任务 
    :param 
    :return taskId : string 创建成功的任务ID
    """
    url = "https://api.yescaptcha.com/createTask"
    data = {
        "clientKey": clientKey,
        "task": {
            "websiteURL": websiteURL,
            "websiteKey": websiteKey,
            "type": task_type
        }
    }
    try:
        # 发送JSON格式的数据
        result = requests.post(url, json=data, verify=False).json()
        taskId = result.get('taskId')
        if taskId is not None:
            return taskId
        print(result)
        
    except Exception as e:
        print(e)


def get_response(taskID: str):
    """ 
    第二步：使用taskId获取response 
    :param taskID: string
    :return response: string 识别结果
    """
    
    # 循环请求识别结果，3秒请求一次
    times = 0
    while times < 120:
        try:
            url = f"https://api.yescaptcha.com/getTaskResult"
            data = {
                "clientKey": clientKey,
                "taskId": taskID
            }
            result = requests.post(url, json=data, verify=False).json()
            solution = result.get('solution', {})
            if solution:
                response = solution.get('gRecaptchaResponse')
                if response:
                    return response
            print(result)
        except Exception as e:
            print(e)

        times += 3
        time.sleep(3)
        

def verify_website(response):
    """ 
    第三步：提交给网站进行验证 
    :param response: string
    :return html: string
    """
    
    # 如果报错：Message: 'geckodriver' executable needs to be in PATH
    # 参考解决：https://www.jianshu.com/p/1d177b266fd2
    # driver = webdriver.Firefox(executable_path=r'C:\Program Files\Mozilla Firefox\geckodriver.exe')
    
    driver = webdriver.Firefox()
    driver.get("https://www.google.com/recaptcha/api2/demo")
    
    # 每个网站的处理方式不同，但是大概思路是一样的
    # 无外乎拿到验证码识别结果，然后想办法提交
    # JS回调就是提交的一种
    # 以下步骤请先看看官方官网的代码，
    # 理解一下三个步骤
    
    # 在网页上执行JS，将获得的验证码写入网页
    driver.execute_script(f'document.getElementById("g-recaptcha-response").value="{response}"')
    
    # 执行回调函数，每个网站回调函数并不相同，需要自己找一下
    # 一般为data-callback=xxxx，这个xxxx就是回调函数
    driver.execute_script(f'onSuccess("{response}")')
    
    # 点击提交
    driver.find_element_by_id("recaptcha-demo-submit").click()
    return driver.page_source



if __name__ == '__main__':

    taskId = create_task()
    print('创建任务:', taskId)
    if taskId is not None:
        response = get_response(taskId)
        print('识别结果:', response)
        result = verify_website(response)
        print('验证结果：', result)
