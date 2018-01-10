def logger_output(logger, log_type, message):
    if logger:
        if log_type == 'info':
            logger.info(message)
        if log_type == 'warn':
            logger.warn(message)
