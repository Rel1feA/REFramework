# 框架使用注意事项  
## 常见问题：打开REDebugger后会报资源丢失错误  
**解决方案**  
1.确保项目已经导入TextMeshPro包，因为编译器拓展用到了TMP的字体  
2.在 Project 窗口中找到框架内的任意 `.uss` 文件  
3.右键 → **Reimport**  